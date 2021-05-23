using Diplomacy.Costs;
using Diplomacy.Extensions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar
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
                            GameTexts.FindText("str_ok", null).ToString(),
                            null,
                            null,
                            null), true);
            }
        }

        public static bool ShouldApply(RebelFaction rebelFaction)
        {
            Clan clan = rebelFaction.SponsorClan;
            var kingdom = clan.Kingdom;
            if (kingdom.IsRebelKingdom() // rebel kingdoms can't have factions
                || !RebelFactionManager.CanStartRebelFaction(rebelFaction.SponsorClan, out _) // check if it's viable to start a faction
                || RebelFactionManager.GetRebelFaction(kingdom).Where(x => x.SponsorClan == clan).Any()) // faction sponsors can't start another faction 
            {
                return false;
            }

            var score = RebelFactionScoringModel.GetDemandScore(clan, rebelFaction);
            return score.ResultNumber >= RebelFactionScoringModel.RequiredScore && new InfluenceCost(clan, InfluenceCost).CanPayCost();
        }
    }
}
