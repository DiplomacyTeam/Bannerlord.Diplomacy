using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace DiplomacyFixes.Alliance
{
    class DeclareAllianceAction
    {
        public static void Apply(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            Kingdom playerKingdom = Clan.PlayerClan?.Kingdom;
            if (otherKingdom == playerKingdom && playerKingdom.Leader == Hero.MainHero)
            {
                TextObject textObject = new TextObject("{=QbOqatd7}{KINGDOM} is proposing an alliance with {PLAYER_KINGDOM}.");
                textObject.SetTextVariable("KINGDOM", kingdom.Name);
                textObject.SetTextVariable("PLAYER_KINGDOM", otherKingdom.Name);

                InformationManager.ShowInquiry(new InquiryData(
                    new TextObject("{=3pbwc8sh}Alliance Proposal").ToString(),
                    textObject.ToString(),
                    true,
                    true,
                    new TextObject("{=3fTqLwkC}Accept").ToString(),
                    new TextObject("{=dRoMejb0}Decline").ToString(),
                    () => ApplyInternal(kingdom, otherKingdom, forcePlayerCharacterCosts),
                    null,
                    ""), true);
            }
            else
            {
                ApplyInternal(kingdom, otherKingdom, forcePlayerCharacterCosts);
            }
        }

        private static void ApplyInternal(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts)
        {
            FactionManager.DeclareAlliance(kingdom, otherKingdom);
            float influenceCost = DiplomacyCostCalculator.DetermineInfluenceCostForFormingAlliance(kingdom, otherKingdom, forcePlayerCharacterCosts);
            if (forcePlayerCharacterCosts)
            {
                DiplomacyCostManager.deductInfluenceFromPlayerClan(influenceCost);
            }
            else
            {
                DiplomacyCostManager.DeductInfluenceFromKingdom(kingdom, influenceCost);
            }
            Events.Instance.OnAllianceFormed(new AllianceFormedEvent(kingdom, otherKingdom));
        }
    }
}
