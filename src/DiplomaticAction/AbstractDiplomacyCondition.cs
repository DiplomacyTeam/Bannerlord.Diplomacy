using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction
{
    internal abstract class AbstractDiplomacyCondition
    {
        internal virtual DiplomacyConditionType ConditionType => DiplomacyConditionType.ActionSpecific;

        public virtual bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, DiplomacyConditionType accountedConditions = DiplomacyConditionType.All,
                                           bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            if (!accountedConditions.HasFlag(ConditionType))
            {
                textObject = null;
                return true;
            }
            else
            {
                return ApplyConditionInternal(kingdom, otherKingdom, out textObject, forcePlayerCharacterCosts, kingdomPartyType);
            }
        }

        protected abstract bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer);
    }
}
