using Diplomacy.Extensions;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.GenericConditions
{
    internal class NotRebelKingdomCondition : AbstractDiplomacyCondition
    {
        private static readonly TextObject _TRebelKingdom = new("{=o0HIAHqg}Rebel kingdoms can't be the target of diplomatic actions.");

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
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