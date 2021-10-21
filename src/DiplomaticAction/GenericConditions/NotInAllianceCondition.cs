using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.GenericConditions
{
    internal sealed class NotInAllianceCondition : AbstractDiplomacyCondition
    {
        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            textObject = null;
            var alreadyInAlliance = FactionManager.IsAlliedWithFaction(kingdom, otherKingdom);
            if (alreadyInAlliance)
            {
                textObject = new TextObject(StringConstants.InAlliance);
            }
            return !alreadyInAlliance;
        }
    }
}
