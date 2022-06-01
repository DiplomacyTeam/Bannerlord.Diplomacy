using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Alliance.Conditions
{
    class AlliancesEnabledCondition : IDiplomacyCondition
    {
        private static readonly TextObject _TAlliancesDisabled = new("{=Dirltd6Z}Alliances are disabled.");

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
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