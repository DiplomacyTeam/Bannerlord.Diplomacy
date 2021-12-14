using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction
{
    internal abstract class AbstractCostCondition : AbstractDiplomacyCondition
    {
        internal override DiplomacyConditionType ConditionType => DiplomacyConditionType.CostRelated;

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            textObject = null;
            var canPayCost = CanPayCost(kingdom, otherKingdom, forcePlayerCharacterCosts, kingdomPartyType);
            if (!canPayCost)
            {
                textObject = GetFailedConditionText();
            }
            return canPayCost;
        }

        protected abstract bool CanPayCost(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer);
        protected abstract TextObject GetFailedConditionText();
    }
}