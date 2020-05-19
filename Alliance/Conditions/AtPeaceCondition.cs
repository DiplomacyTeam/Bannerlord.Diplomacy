using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.Alliance.Conditions
{
    class AtPeaceCondition : IDiplomacyCondition
    {
        private const string AT_WAR = "{=UQZmdLzc}Cannot be at war.";
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            bool atWar = FactionManager.IsAtWarAgainstFaction(kingdom, otherKingdom);
            if (atWar)
            {
                textObject = new TextObject(AT_WAR);
            }
            return !atWar;
        }
    }
}
