using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace.Conditions
{
    class HasEnoughGoldForPeaceCondition : AbstractCostCondition
    {
        protected override TextObject FailedConditionText => new(StringConstants.NOT_ENOUGH_GOLD);

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, ref TextObject? textObject, bool forcePlayerCharacterCosts = false)
        {
            var hasEnoughGold = DiplomacyCostCalculator.DetermineCostForMakingPeace(kingdom, otherKingdom, forcePlayerCharacterCosts).GoldCost.CanPayCost();
            if (!hasEnoughGold)
            {
                textObject = FailedConditionText;
            }
            return hasEnoughGold;
        }
    }
}
