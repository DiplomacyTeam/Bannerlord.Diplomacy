using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.GenericConditions
{
    internal sealed class NotEliminatedCondition : IDiplomacyCondition
    {
        public bool ApplyCondition(Kingdom kingdom,
                                   Kingdom otherKingdom,
                                   out TextObject? textObject,
                                   bool forcePlayerCosts = false,
                                   bool bypassCosts = false)
        {
            textObject = null;

            var eliminated = !(kingdom.IsEliminated || otherKingdom.IsEliminated);

            if (!eliminated)
                textObject = new TextObject("{=lQAaLeSy}Faction has been eliminated.");

            return eliminated;
        }
    }
}