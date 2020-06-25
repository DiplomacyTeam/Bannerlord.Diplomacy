using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.NonAggressionPact
{
    internal class HasEnoughInfluenceCondition : IDiplomacyCondition
    {
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            DiplomacyCost influenceCost = DiplomacyCostCalculator.DetermineCostForFormingNonAggressionPact(kingdom, otherKingdom, forcePlayerCharacterCosts).InfluenceCost;
            bool hasEnoughInfluence = influenceCost.CanPayCost();

            if (!hasEnoughInfluence)
            {
                textObject = new TextObject(StringConstants.NOT_ENOUGH_INFLUENCE);
            }

            return hasEnoughInfluence;
        }
    }
}