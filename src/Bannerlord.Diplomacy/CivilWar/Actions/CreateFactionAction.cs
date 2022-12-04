using Diplomacy.CivilWar.Factions;
using Diplomacy.CivilWar.Scoring;
using Diplomacy.Costs;
using Diplomacy.Extensions;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar.Actions
{
    public class CreateFactionAction
    {
        private const float InfluenceCost = 100f;

        public static void Apply(RebelFaction rebelFaction)
        {
            var kingdom = rebelFaction.ParentKingdom;
            var clan = rebelFaction.SponsorClan;
            RebelFactionManager.RegisterRebelFaction(rebelFaction);
            new InfluenceCost(clan, InfluenceCost).ApplyCost();
            if (kingdom == Hero.MainHero.MapFaction)
            {
                var strVars = new Dictionary<string, object> {
                        { "KINGDOM", rebelFaction.ParentKingdom.Name },
                        { "CLAN_NAME", rebelFaction.SponsorClan.Name },
                        { "DEMAND", rebelFaction.DemandDescription }
                    };
                InformationManager.ShowInquiry(
                        new InquiryData(
                            new TextObject("{=1OQGJkqb}A Rebel Faction Emerges").ToString(),
                            new TextObject("{=pqjZ6wck}A rebel faction has emerged in {KINGDOM}. They are led by clan {CLAN_NAME}. {DEMAND}", strVars).ToString(),
                            true,
                            false,
                            GameTexts.FindText("str_ok").ToString(),
                            null,
                            null,
                            null), true);
            }
        }

        public static bool ShouldApply(RebelFaction rebelFaction)
        {
            var clan = rebelFaction.SponsorClan;
            if (!CanApply(clan, default, out _))
            {
                return false;
            }

            var score = RebelFactionScoringModel.GetDemandScore(clan, rebelFaction);
            return score.ResultNumber >= RebelFactionScoringModel.RequiredScore && new InfluenceCost(clan, InfluenceCost).CanPayCost();
        }

        public static bool CanApply(Clan clan, RebelDemandType? demandType, out TextObject? reason)
        {
            IEnumerable<TextObject> exceptions;
            reason = (exceptions = CanApply(clan, demandType)).FirstOrDefault();

            return !exceptions.Any();
        }

        public static IEnumerable<TextObject> CanApply(Clan clan, RebelDemandType? demandType)
        {
            if (clan.Kingdom == null)
            {
                yield return TextObject.Empty;
            }

            if (clan.Kingdom!.IsRebelKingdom())
            {
                yield return new TextObject("{=luvsD6Zn}Cannot create a faction in a rebel kingdom.");
            }

            // ruling clan can't create factions
            if (clan == clan.Kingdom!.RulingClan)
            {
                yield return new TextObject("{=quo5erz6}Rulers cannot create factions.");
            }

            if (clan.IsUnderMercenaryService)
            {
                yield return new TextObject("{=JDk8ustS}Mercenaries cannot create factions.");
            }

            // must be clan tier 4+ to start a secession faction
            if (demandType == RebelDemandType.Secession && clan.Tier < 4)
            {
                yield return new TextObject("{=iXMeCmj0}Must be Clan Tier 4 or higher.");
            }

            // rebel kingdoms can't have factions

            if (RebelFactionManager.AllRebelFactions.TryGetValue(clan.Kingdom, out var rebelFactions))
            {
                // no active rebellions
                if (rebelFactions.Any(x => x.AtWar))
                {
                    yield return new TextObject("{=ovgs58sT}Can't start a faction during an active rebellion.");
                }

                // faction sponsors can't start another faction 
                if (rebelFactions.Any(x => x.SponsorClan == clan))
                {
                    yield return new TextObject("{=zTZWwJfN}You cannot lead two factions.");
                }

                // players can exceed the max
                if (rebelFactions.Count >= 3 && clan != Clan.PlayerClan)
                {
                    yield return TextObject.Empty;
                }

                // only one abdication faction allowed
                if (demandType == RebelDemandType.Abdication && rebelFactions.Any(x => x.RebelDemandType == RebelDemandType.Abdication))
                {
                    yield return new TextObject("{=DxJH7ciY}A kingdom can only have one abdication faction.");
                }
            }

            if (RebelFactionManager.Instance!.LastCivilWar.TryGetValue(clan.Kingdom, out var lastTime))
            {
                var daysSinceLastCivilWar = lastTime.ElapsedDaysUntilNow;

                if (daysSinceLastCivilWar < Settings.Instance!.MinimumTimeSinceLastCivilWarInDays)
                {
                    yield return new TextObject("{=VbpiW2bd}Can't start a faction so soon after a civil war.");
                }
            }

            if (!new InfluenceCost(clan, Settings.Instance!.FactionCreationInfluenceCost).CanPayCost())
            {
                yield return new TextObject(StringConstants.NotEnoughInfluence);
            }
        }
    }
}