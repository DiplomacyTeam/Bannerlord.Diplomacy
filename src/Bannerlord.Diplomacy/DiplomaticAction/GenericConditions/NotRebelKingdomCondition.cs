using Diplomacy.Extensions;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.GenericConditions
{
    internal class NotRebelKingdomCondition : IDiplomacyCondition
    {
        private static readonly TextObject _TRebelKingdom = new("{=o0HIAHqg}Rebel kingdoms can't be the target of diplomatic actions.");

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            textObject = null;
            if (kingdom.IsRebelKingdom() || otherKingdom.IsRebelKingdom())
            {
                textObject = _TRebelKingdom;
                return false;
            }
            return true;
        }
    }
}