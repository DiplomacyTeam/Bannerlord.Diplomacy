using Diplomacy.Costs;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace.Conditions
{
    class HasEnoughInfluenceForWarCondition : AbstractCostCondition
    {
        protected override TextObject FailedConditionText => new(StringConstants.NotEnoughInfluence);

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, ref TextObject? textObject, bool forcePlayerCharacterCosts = false)
        {
            var hasEnoughInfluence = DiplomacyCostCalculator.DetermineCostForDeclaringWar(kingdom, forcePlayerCharacterCosts).CanPayCost();
            if (!hasEnoughInfluence)
            {
                textObject = FailedConditionText;
            }
            return hasEnoughInfluence;
        }
    }
}