using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.WarPeace.Conditions
{
    class HasEnoughInfluenceForPeaceCondition : IDiplomacyCondition
    {
        private const string NOT_ENOUGH_INFLUENCE = "{=TS1iV2pO}Not enough influence!";

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            Hero heroPayingCosts = forcePlayerCharacterCosts ? Hero.MainHero : kingdom.Leader;
            bool hasEnoughInfluence = heroPayingCosts.Clan.Influence >= DiplomacyCostCalculator.DetermineInfluenceCostForMakingPeace(kingdom);
            if (!hasEnoughInfluence)
            {
                textObject = new TextObject(NOT_ENOUGH_INFLUENCE);
            }
            return hasEnoughInfluence;
        }
    }
}
