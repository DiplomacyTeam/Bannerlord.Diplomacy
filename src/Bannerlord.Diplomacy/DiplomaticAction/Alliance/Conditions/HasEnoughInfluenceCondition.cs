using Diplomacy.Costs;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Alliance.Conditions
{
    class HasEnoughInfluenceCondition : AbstractCostCondition
    {
        protected override TextObject FailedConditionText => new(StringConstants.NotEnoughInfluence);

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, ref TextObject? textObject, bool forcePlayerCharacterCosts = false)
        {
            var hasEnoughInfluence = DiplomacyCostCalculator.DetermineCostForFormingAlliance(kingdom, otherKingdom, forcePlayerCharacterCosts).InfluenceCost.CanPayCost();
            if (!hasEnoughInfluence)
            {
                textObject = FailedConditionText;
            }

            return hasEnoughInfluence;
        }
    }
}