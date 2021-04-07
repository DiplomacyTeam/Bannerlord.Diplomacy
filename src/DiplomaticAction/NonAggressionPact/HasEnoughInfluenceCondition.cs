using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.NonAggressionPact
{
    internal class HasEnoughInfluenceCondition : AbstractCostCondition
    {
        protected override TextObject FailedConditionText => new TextObject(StringConstants.NOT_ENOUGH_INFLUENCE);

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, ref TextObject? textObject, bool forcePlayerCharacterCosts = false)
        {
            DiplomacyCost influenceCost = DiplomacyCostCalculator.DetermineCostForFormingNonAggressionPact(kingdom, otherKingdom, forcePlayerCharacterCosts).InfluenceCost;
            var hasEnoughInfluence = influenceCost.CanPayCost();

            if (!hasEnoughInfluence)
            {
                textObject = FailedConditionText;
            }

            return hasEnoughInfluence;
        }
    }
}
