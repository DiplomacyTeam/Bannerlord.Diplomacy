using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Diplomacy.WarExhaustion.EventRecords
{
    internal sealed class SummaryCasualtyRecord : CasualtyRecord
    {
        [SaveableField(20)]
        private readonly CampaignTime _summingUpDate;

        public CampaignTime SummingUpDate => _summingUpDate;

        public SummaryCasualtyRecord(CampaignTime eventDate, CampaignTime summingUpDate, int faction1Value, float faction1ExhaustionValue, int faction2Value, float faction2ExhaustionValue) : base(eventDate, faction1Value, faction1ExhaustionValue, faction2Value, faction2ExhaustionValue)
        {
            _summingUpDate = summingUpDate;
        }

        protected override TextObject? GetEventDescription(int factionIndex)
        {
            var factionValue = factionIndex switch
            {
                1 => _faction1Value,
                2 => _faction2Value,
                _ => throw new ArgumentOutOfRangeException(nameof(factionIndex)),
            };

            TextObject? textObject = factionValue != 0 ? FindTextWithVariation("str_war_exhaustion_casualty.0") : null;
            if (textObject is not null)
            {
                SetDescriptionVariables(textObject, factionValue);
            }
            return textObject;
        }

        private new void SetDescriptionVariables(TextObject textObject, float factionValue)
        {
            textObject.SetTextVariable("START_DATE", _eventDate.ToString());
            textObject.SetTextVariable("END_DATE", _summingUpDate.ToString());

            base.SetDescriptionVariables(textObject, factionValue);
        }
    }
}