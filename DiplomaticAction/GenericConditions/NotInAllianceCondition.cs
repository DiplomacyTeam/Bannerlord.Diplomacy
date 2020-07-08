using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.GenericConditions
{
    class NotInAllianceCondition : IDiplomacyCondition
    {

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            textObject = null;
            bool alreadyInAlliance = FactionManager.IsAlliedWithFaction(kingdom, otherKingdom);
            if (alreadyInAlliance)
            {
                textObject = new TextObject(StringConstants.IN_ALLIANCE);
            }
            return !alreadyInAlliance;
        }
    }
}
