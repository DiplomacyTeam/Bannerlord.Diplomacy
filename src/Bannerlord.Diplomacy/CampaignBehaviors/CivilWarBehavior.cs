
using Diplomacy.CivilWar;
using Diplomacy.CivilWar.Actions;
using Diplomacy.CivilWar.Factions;
using Diplomacy.CivilWar.Scoring;
using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class CivilWarBehavior : CampaignBehaviorBase
    {
        private RebelFactionManager _rebelFactionManager;

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, DailyTickClan);
            CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, (x, y, z, _, _) => RemoveClanFromRebelFaction(x, y, z));
            CampaignEvents.MakePeace.AddNonSerializedListener(this, ResolveCivilWar);
            CampaignEvents.KingdomDecisionConcluded.AddNonSerializedListener(this, NewKing);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
        }

        public CivilWarBehavior()
        {
            _rebelFactionManager = new RebelFactionManager();
        }

        private void DailyTick()
        {
            IEnumerable<RebelFaction> expiredFactions = RebelFactionManager.AllRebelFactions.Values.SelectMany(x => x).Where(x => x.DateStarted.ElapsedDaysUntilNow > Settings.Instance!.MaximumFactionDurationInDays);
            foreach (RebelFaction rebelFaction in expiredFactions.ToList())
            {
                if (rebelFaction.AtWar)
                    continue;
                RebelFactionManager.DestroyRebelFaction(rebelFaction);
            }

            // if war exhaustion is disabled, stop civil wars when the last rebel settlement is taken
            if (!Settings.Instance!.EnableWarExhaustion)
            {
                foreach (var kingdom in KingdomExtensions.AllActiveKingdoms.Where(x => (x.IsRebelKingdom() || x.HasRebellion()) && x.Fiefs.IsEmpty()).ToList())
                {
                    var rebelFaction = RebelFactionManager.GetRebelFactionForRebelKingdom(kingdom) ?? kingdom.GetRebelFactions().FirstOrDefault();

                    if (rebelFaction != null)
                    {
                        Kingdom otherKingdom = kingdom.IsRebelKingdom() ? rebelFaction.ParentKingdom : rebelFaction.RebelKingdom!;
                        KingdomPeaceAction.ApplyPeace(kingdom, otherKingdom);
                    }
                }
            }
        }

        private void NewKing(KingdomDecision decision, DecisionOutcome chosenOutcome, bool arg3)
        {
            if (decision is not KingSelectionKingdomDecision)
            {
                return;
            }

            var kingdom = decision.Kingdom;
            var newKing = kingdom.Leader;

            foreach (RebelFaction rebelFaction in RebelFactionManager.GetRebelFaction(kingdom))
            {
                if (rebelFaction is AbdicationFaction abdicationFaction)
                {
                    abdicationFaction.DestroyFactionBecauseDemandSatisfied();
                }
                rebelFaction.RemoveClan(newKing.Clan);
            }
        }

        private void ResolveCivilWar(IFaction loser, IFaction winner)
        {
            if (loser is Kingdom kingdom1 && winner is Kingdom kingdom2)
            {
                if (kingdom1.IsRebelKingdom() || kingdom2.IsRebelKingdom())
                {
                    var rebelKingdom = kingdom1.IsRebelKingdom() ? kingdom1 : kingdom2;
                    var parentKingdom = kingdom1.IsRebelKingdom() ? kingdom2 : kingdom1;
                    var rebelFaction = RebelFactionManager.GetRebelFaction(parentKingdom).FirstOrDefault(x => x.RebelKingdom == rebelKingdom);

                    if (rebelFaction is null)
                        return;
                    if (loser == rebelKingdom)
                        rebelFaction.EnforceFailure();
                    else
                        rebelFaction.EnforceSuccess();
                }
            }
        }
        private void RemoveClanFromRebelFaction(Clan clan, Kingdom oldKingdom, Kingdom newKingdom)
        {
            var rebelFactions = RebelFactionManager.GetRebelFaction(oldKingdom);
            foreach (RebelFaction rf in rebelFactions)
            {
                if (newKingdom != null)
                {
                    if (rf!.RebelKingdom == newKingdom || (rf.RebelKingdom is null && rf.SponsorClan == clan && newKingdom.Clans.Count == 1))
                        return;
                }
                rf.RemoveClan(clan);
            }
        }

        private void DailyTickClan(Clan clan)
        {
            // don't consider civil war if not in a kingdom or this clan is the kingdom leader
            if (!clan.MapFaction.IsKingdomFaction || clan.MapFaction.Leader == clan.Leader || clan.IsMinorFaction || clan.IsUnderMercenaryService || clan.Leader.IsHumanPlayerCharacter || clan.Kingdom.IsRebelKingdom())
                return;

            Kingdom kingdom = (clan.MapFaction as Kingdom)!;
            IEnumerable<RebelFaction> rebelFactions = RebelFactionManager.GetRebelFaction(kingdom)!;

            // active rebellion
            if (rebelFactions.Any(x => x.AtWar))
            {
                return;
            }

            var ownFaction = clan.GetSponsoredRebelFaction();
            var rand = MBRandom.RandomFloat;

            if (ownFaction is null)
            {
                // consider leaving existing rebel factions
                ConsiderLeavingRebelFaction(clan);

                // considering joining existing rebel factions
                if (rand < Settings.Instance!.DailyChanceToJoinRebelFaction)
                {
                    ConsiderJoiningRebelFaction(clan);
                }

                // consider creating a rebel faction
                if (clan.GetRebelFactions().IsEmpty() && rand < Settings.Instance!.DailyChanceToStartRebelFaction)
                {
                    ConsiderCreatingRebelFaction(kingdom, clan);
                }
            }
            else
            {
                // if rebel faction has enough support, start rebellion
                if (!ownFaction.AtWar && ownFaction.HasCriticalSupport && rand < Settings.Instance!.DailyChanceToStartCivilWar)
                {
                    StartRebellionAction.Apply(ownFaction);
                    // need to return as starting a rebellion eliminates all other factions and we don't want to continue calculating
                }
            }
        }

        private void ConsiderLeavingRebelFaction(Clan clan)
        {
            foreach (RebelFaction faction in RebelFactionManager.GetRebelFaction(clan.Kingdom).Where(x => x.Clans.Contains(clan) && clan != x.SponsorClan))
            {
                if (!JoinFactionAction.ShouldApply(clan, faction))
                {
                    LeaveFactionAction.Apply(clan, faction);
                }
            }
        }

        private void ConsiderJoiningRebelFaction(Clan clan)
        {
            List<Tuple<RebelFaction, ExplainedNumber>> scores = new();
            foreach (RebelFaction faction in RebelFactionManager.GetRebelFaction(clan.Kingdom).Where(x => !x.Clans.Contains(clan)))
            {
                scores.Add(new Tuple<RebelFaction, ExplainedNumber>(faction, RebelFactionScoringModel.GetDemandScore(clan, faction)));
            }

            var bestFactionToJoin = scores.OrderByDescending(x => x.Item2.ResultNumber).FirstOrDefault()?.Item1;
            if (bestFactionToJoin != null)
            {
                var shouldJoinFaction = JoinFactionAction.ShouldApply(clan, bestFactionToJoin);

                // score + random chance
                if (shouldJoinFaction)
                {
                    JoinFactionAction.Apply(clan, bestFactionToJoin);
                }
            }
        }

        private void ConsiderCreatingRebelFaction(Kingdom kingdom, Clan clan)
        {
            // don't add new rebel faction if one exists
            if (kingdom.IsRebelKingdom() || !CreateFactionAction.CanApply(clan, default, out _))
                return;

            var rebelFactionScore = RebelFactionScoringModel.GetDemandScore(clan).OrderByDescending(x => x.Value.ResultNumber).First();

            bool shouldStartFaction = CreateFactionAction.ShouldApply(rebelFactionScore.Key);
            // score + random chance
            if (shouldStartFaction)
            {
                CreateFactionAction.Apply(rebelFactionScore.Key);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_rebelFactionManager", ref _rebelFactionManager);

            if (dataStore.IsLoading)
            {
                _rebelFactionManager ??= new();
                _rebelFactionManager.Sync();
            }
        }
    }
}
