using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.GenericConditions
{
    internal sealed class AtPeaceCondition : AbstractDiplomacyCondition
    {
        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
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