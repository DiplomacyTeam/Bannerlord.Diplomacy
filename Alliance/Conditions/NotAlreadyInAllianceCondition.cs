using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.Alliance.Conditions
{
    class NotAlreadyInAllianceCondition : IDiplomacyCondition
    {
        private const string ALREADY_IN_ALLIANCE = "{=KHJyWj9H}Cannot already be in an alliance.";
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            bool alreadyInAlliance = FactionManager.IsAlliedWithFaction(kingdom, otherKingdom);
            if (alreadyInAlliance)
            {
                textObject = new TextObject(ALREADY_IN_ALLIANCE);
            }
            return !alreadyInAlliance;
        }
    }
}
