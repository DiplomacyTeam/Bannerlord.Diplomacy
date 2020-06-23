using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.GenericConditions
{
    class AtPeaceCondition : IDiplomacyCondition
    {
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            bool atWar = FactionManager.IsAtWarAgainstFaction(kingdom, otherKingdom);
            if (atWar)
            {
                textObject = new TextObject(StringConstants.AT_WAR);
            }
            return !atWar;
        }
    }
}
