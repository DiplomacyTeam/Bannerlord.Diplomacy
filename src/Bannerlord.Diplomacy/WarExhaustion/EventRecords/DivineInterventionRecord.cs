using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Diplomacy.WarExhaustion.EventRecords
{
    internal sealed class DivineInterventionRecord : WarExhaustionEventRecord
    {
        [SaveableField(65)]
        private readonly TextObject _faction1Name;
        [SaveableField(66)]
        private readonly TextObject _faction2Name;

        public override WarExhaustionType WarExhaustionType => WarExhaustionType.Divine;
        public TextObject Faction1Name => _faction1Name;
        public TextObject Faction2Name => _faction2Name;

        public DivineInterventionRecord(CampaignTime eventDate, int faction1Value, float faction1ExhaustionValue, int faction2Value, float faction2ExhaustionValue, TextObject faction1Name, TextObject faction2Name)
            : base(eventDate, faction1Value, faction1ExhaustionValue, faction2Value, faction2ExhaustionValue)
        {
            _faction1Name = faction1Name;
            _faction2Name = faction2Name;
        }

        protected override TextObject? GetEventDescription(int factionIndex)
        {
            var (factionEffected, factionExhaustionValue, factionName) = factionIndex switch
            {
                1 => (Faction1Effected, _faction1ExhaustionValue, _faction1Name),
                2 => (Faction2Effected, _faction2ExhaustionValue, _faction2Name),
                _ => throw new ArgumentOutOfRangeException(nameof(factionIndex)),
            };

            TextObject? textObject = factionEffected ? GameTexts.FindText("str_war_exhaustion_divine", factionExhaustionValue <= -0.1f ? "0" : factionExhaustionValue >= 0.1f ? "2" : "1") : null;
            if (textObject is not null)
            {
                SetDescriptionVariables(textObject, factionName);
            }
            return textObject;
        }

        private void SetDescriptionVariables(TextObject textObject, TextObject factionName)
        {
            textObject.SetTextVariable("DATE_STR", _eventDate.ToString());
            textObject.SetTextVariable("FACTION", factionName);
        }
    }
}