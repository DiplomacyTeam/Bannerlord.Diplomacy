using Bannerlord.ButterLib.Common.Helpers;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.WarExhaustion.EventRecords
{
    internal abstract class CasualtyRecord : WarExhaustionEventRecord
    {
        public override WarExhaustionType WarExhaustionType => WarExhaustionType.Casualty;
        public int Faction1Casualties => _faction1Value;
        public int Faction2Casualties => _faction2Value;

        public CasualtyRecord(CampaignTime eventDate, int faction1Value, float faction1ExhaustionValue, int faction2Value, float faction2ExhaustionValue) : base(eventDate, faction1Value, faction1ExhaustionValue, faction2Value, faction2ExhaustionValue) { }

        protected void SetDescriptionVariables(TextObject textObject, float factionValue)
        {
            LocalizationHelper.SetNumericVariable(textObject, "VALUE", factionValue);
        }
    }
}