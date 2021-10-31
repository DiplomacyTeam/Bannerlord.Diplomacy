using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.GenericConditions
{
    internal sealed class HasAuthorityCondition : AbstractDiplomacyCondition
    {
        private static readonly TextObject _TNoAuthority = new("{=lQAaLeSy}You don't have the authority to perform this action.");

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            textObject = null;
            var authority = Clan.PlayerClan.Kingdom != kingdom
                            || (kingdom?.Leader.IsHumanPlayerCharacter ?? false)
                            || !forcePlayerCharacterCosts
                            || Settings.Instance!.PlayerDiplomacyControl;
            if (!authority)
            {
                textObject = _TNoAuthority;
            }
            return authority;
        }
    }
}
