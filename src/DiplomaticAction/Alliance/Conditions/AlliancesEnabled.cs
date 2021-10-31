using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Alliance.Conditions
{
    internal sealed class AlliancesEnabledCondition : AbstractDiplomacyCondition
    {
        private static readonly TextObject _TAlliancesDisabled = new("{=Dirltd6Z}Alliances are disabled.");

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            textObject = null;
            var alliancesEnabled = Settings.Instance!.EnableAlliances;
            if (!alliancesEnabled)
            {
                textObject = _TAlliancesDisabled;
            }
            return alliancesEnabled;
        }
    }
}
