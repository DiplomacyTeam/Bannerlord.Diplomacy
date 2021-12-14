using Diplomacy.Costs;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.NonAggressionPact
{
    class HasEnoughGoldCondition : AbstractCostCondition
    {
        protected override TextObject FailedConditionText => new(StringConstants.NotEnoughGold);

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, ref TextObject? textObject, bool forcePlayerCharacterCosts = false)
        {
            var hasEnoughGold = DiplomacyCostCalculator.DetermineCostForFormingNonAggressionPact(kingdom, otherKingdom, forcePlayerCharacterCosts).GoldCost.CanPayCost();
            if (!hasEnoughGold)
            {
                textObject = FailedConditionText;
            }
            return hasEnoughGold;
        }
    }
}