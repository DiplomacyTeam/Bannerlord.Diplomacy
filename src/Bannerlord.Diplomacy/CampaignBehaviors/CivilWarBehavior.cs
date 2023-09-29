using Diplomacy.CivilWar;
using Diplomacy.CivilWar.Actions;
using Diplomacy.CivilWar.Factions;
using Diplomacy.CivilWar.Scoring;
using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.Extensions;
using Diplomacy.WarExhaustion;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
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
#if v124
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, (x, y, z, _, _) => RemoveClanFromRebelFaction(x, y, z));
#elif v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115 || v116 || v120 || v121 || v122 || v123
            CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, (x, y, z, _, _) => RemoveClanFromRebelFaction(x, y, z));
#endif
            CampaignEvents.MakePeace.AddNonSerializedListener(this, ResolveCivilWar);
            CampaignEvents.KingdomDecisionConcluded.AddNonSerializedListener(this, NewKing);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.OnClanDestroyedEvent.AddNonSerializedListener(this, OnClanDesroyed);
            CampaignEvents.KingdomDestroyedEvent.AddNonSerializedListener(this, OnKingdomDestroyed);

            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoadFinished);
        }

        public CivilWarBehavior()
        {
            _rebelFactionManager = new RebelFactionManager();
        }

        private void DailyTick()
        {
            var expiredFactions = RebelFactionManager.AllRebelFactions.Values.SelectMany(x => x).Where(x => x.DateStarted.ElapsedDaysUntilNow > Settings.Instance!.MaximumFactionDurationInDays).Distinct().ToList();
            foreach (var rebelFaction in expiredFactions)
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

                    var otherKingdom = kingdom.IsRebelKingdom() ? rebelFaction.ParentKingdom : rebelFaction.RebelKingdom!;
                    if (rebelFaction != null)
                    {
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

            foreach (var rebelFaction in RebelFactionManager.GetRebelFaction(kingdom).ToList())
            {
                if (rebelFaction is AbdicationFaction abdicationFaction)
                {
                    abdicationFaction.DestroyFactionBecauseDemandSatisfied();
                }
                rebelFaction.RemoveClan(newKing.Clan);
            }
        }

#if v100 || v101 || v102 || v103
        private void ResolveCivilWar(IFaction factionMakingPeace, IFaction otherFaction)
#else
        private void ResolveCivilWar(IFaction factionMakingPeace, IFaction otherFaction, MakePeaceAction.MakePeaceDetail makePeaceDetail)
#endif
        {
            //Need to check if this runs before or after WarExhaustionBehavior
            if (factionMakingPeace is Kingdom kingdomMakingPeace && otherFaction is Kingdom otherKingdom)
            {
                var kingdomMakingPeaceIsRebel = kingdomMakingPeace.IsRebelKingdomOf(otherKingdom);
                if (kingdomMakingPeaceIsRebel || otherKingdom.IsRebelKingdomOf(kingdomMakingPeace))
                {
                    var rebelKingdom = kingdomMakingPeaceIsRebel ? kingdomMakingPeace : otherKingdom;
                    var parentKingdom = kingdomMakingPeaceIsRebel ? otherKingdom : kingdomMakingPeace;
                    var rebelFaction = RebelFactionManager.GetRebelFaction(parentKingdom).First(x => x.RebelKingdom == rebelKingdom);
                    var loserKingdom = RebelFactionManager.GetCivilWarLoser(kingdomMakingPeace, otherKingdom);
                    ResolveLoss(loserKingdom, rebelKingdom, rebelFaction);
                }
            }

            static void ResolveLoss(IFaction loser, Kingdom rebelKingdom, RebelFaction rebelFaction)
            {
                if (loser == rebelKingdom)
                    rebelFaction.EnforceFailure();
                else
                    rebelFaction.EnforceSuccess();
            }
        }

        private void RemoveClanFromRebelFaction(Clan clan, Kingdom oldKingdom, Kingdom newKingdom)
        {
            var rebelFactions = RebelFactionManager.GetRebelFaction(oldKingdom).ToList();
            foreach (var rf in rebelFactions)
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
            if (!clan.MapFaction.IsKingdomFaction || clan.MapFaction.Leader == clan.Leader || clan.IsMinorFaction || clan.IsUnderMercenaryService || clan.Leader.IsHumanPlayerCharacter || clan.Kingdom.IsRebelKingdom() || clan.IsEliminated)
                return;

            var kingdom = (clan.MapFaction as Kingdom)!;
            var rebelFactions = RebelFactionManager.GetRebelFaction(kingdom)!;

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
            foreach (var faction in RebelFactionManager.GetRebelFaction(clan.Kingdom).Where(x => x.Clans.Contains(clan) && clan != x.SponsorClan).ToList())
            {
                if (!JoinFactionAction.ShouldApply(clan, faction))
                {
                    LeaveFactionAction.Apply(clan, faction);
                }
            }
        }

        private void ConsiderJoiningRebelFaction(Clan clan)
        {
            var scores = new List<Tuple<RebelFaction, ExplainedNumber>>();
            foreach (var faction in RebelFactionManager.GetRebelFaction(clan.Kingdom).Where(x => !x.Clans.Contains(clan)).ToList())
            {
                scores.Add(new(faction, RebelFactionScoringModel.GetDemandScore(clan, faction)));
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

            var shouldStartFaction = CreateFactionAction.ShouldApply(rebelFactionScore.Key);
            // score + random chance
            if (shouldStartFaction)
            {
                CreateFactionAction.Apply(rebelFactionScore.Key);
            }
        }

        private void OnClanDesroyed(Clan destroyedClan)
        {
            foreach (var faction in RebelFactionManager.GetRebelFaction(destroyedClan.Kingdom).Where(x => x.Clans.Contains(destroyedClan)).ToList())
            {
                LeaveFactionAction.Apply(destroyedClan, faction);
            }
        }

        private void OnKingdomDestroyed(Kingdom destroyedKingdom)
        {
            foreach (var faction in RebelFactionManager.GetRebelFaction(destroyedKingdom).ToList())
            {
                RebelFactionManager.DestroyRebelFaction(faction, faction.AtWar);
            }
        }

        private void OnGameLoadFinished() => _rebelFactionManager.OnAfterSaveLoaded();

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