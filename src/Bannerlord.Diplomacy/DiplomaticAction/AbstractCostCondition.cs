using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction
{
    abstract class AbstractCostCondition : IDiplomacyCondition
    {
        protected abstract TextObject FailedConditionText { get; }

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            textObject = null;
            if (bypassCosts)
            {
                return true;
            }
            else
            {
                return ApplyConditionInternal(kingdom, otherKingdom, ref textObject, forcePlayerCharacterCosts);
            }
        }

        protected abstract bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, ref TextObject? textObject, bool forcePlayerCharacterCosts = false);
    }
}