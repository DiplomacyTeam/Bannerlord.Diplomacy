using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.Alliance.Conditions
{
    class AlliancesEnabledCondition : IDiplomacyCondition
    {
        private const string ALLIANCES_DISABLED = "{=Dirltd6Z}Alliances are disabled.";

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            bool alliancesEnabled = Settings.Instance.EnableAlliances;
            if (!alliancesEnabled)
            {
                textObject = new TextObject(ALLIANCES_DISABLED);
            }
            return alliancesEnabled;
        }
    }
}
