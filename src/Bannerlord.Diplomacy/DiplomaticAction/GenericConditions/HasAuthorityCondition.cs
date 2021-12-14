using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.GenericConditions
{
    internal sealed class HasAuthorityCondition : IDiplomacyCondition
    {
        public bool ApplyCondition(Kingdom kingdom,
                                   Kingdom otherKingdom,
                                   out TextObject? textObject,
                                   bool forcePlayerCosts = false,
                                   bool bypassCosts = false)
        {
            textObject = null;

            var authority = Clan.PlayerClan.Kingdom != kingdom
                || (kingdom?.Leader.IsHumanPlayerCharacter ?? false)
                || !forcePlayerCosts
                || Settings.Instance!.PlayerDiplomacyControl;

            if (!authority)
                textObject = new TextObject("{=lQAaLeSy}You don't have the authority to perform this action.");

            return authority;
        }
    }
}