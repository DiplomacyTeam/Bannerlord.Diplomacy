using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.WarPeace.Conditions
{
    class HasEnoughInfluenceForWarCondition : AbstractCostCondition
    {
        protected override TextObject FailedConditionText => new TextObject(StringConstants.NOT_ENOUGH_INFLUENCE);

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, ref TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            bool hasEnoughInfluence = DiplomacyCostCalculator.DetermineCostForDeclaringWar(kingdom, forcePlayerCharacterCosts).CanPayCost();
            if (!hasEnoughInfluence)
            {
                textObject = FailedConditionText;
            }
            return hasEnoughInfluence;
        }
    }
}
