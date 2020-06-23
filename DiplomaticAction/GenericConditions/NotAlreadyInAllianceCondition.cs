using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.GenericConditions
{
    class NotAlreadyInAllianceCondition : IDiplomacyCondition
    {
        
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            bool alreadyInAlliance = FactionManager.IsAlliedWithFaction(kingdom, otherKingdom);
            if (alreadyInAlliance)
            {
                textObject = new TextObject(StringConstants.ALREADY_IN_ALLIANCE);
            }
            return !alreadyInAlliance;
        }
    }
}
