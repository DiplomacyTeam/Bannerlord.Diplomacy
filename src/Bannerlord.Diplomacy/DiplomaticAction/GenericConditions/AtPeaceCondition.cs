using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.GenericConditions
{
    class AtPeaceCondition : IDiplomacyCondition
    {
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            textObject = null;
            var atWar = FactionManager.IsAtWarAgainstFaction(kingdom, otherKingdom);
            if (atWar)
            {
                textObject = new TextObject(StringConstants.AtWar);
            }
            return !atWar;
        }
    }
}