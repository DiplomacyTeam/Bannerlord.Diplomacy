using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Diplomacy.WarExhaustion.EventRecords
{
    internal abstract class WarExhaustionEventRecord
    {
        [SaveableField(1)]
        protected readonly CampaignTime _eventDate;
        [SaveableField(2)]
        protected readonly int _faction1Value;
        [SaveableField(3)]
        protected readonly float _faction1ExhaustionValue;
        [SaveableField(4)]
        protected readonly int _faction2Value;
        [SaveableField(5)]
        protected readonly float _faction2ExhaustionValue;

        public CampaignTime EventDate => _eventDate;
        public abstract WarExhaustionType WarExhaustionType { get; }

        internal int Faction1Value => _faction1Value;
        public bool Faction1Effected => _faction1Value != 0;
        public float Faction1ExhaustionValue => _faction1ExhaustionValue;
        public TextObject? Faction1EventDescription => GetEventDescription(1);

        internal int Faction2Value => _faction2Value;
        public bool Faction2Effected => _faction2Value != 0;
        public float Faction2ExhaustionValue => _faction2ExhaustionValue;
        public TextObject? Faction2EventDescription => GetEventDescription(2);

        public WarExhaustionEventRecord(CampaignTime eventDate, int faction1Value, float faction1ExhaustionValue, int faction2Value, float faction2ExhaustionValue)
        {
            _eventDate = eventDate;
            _faction1Value = faction1Value;
            _faction1ExhaustionValue = faction1ExhaustionValue;
            _faction2Value = faction2Value;
            _faction2ExhaustionValue = faction2ExhaustionValue;
        }

        protected abstract TextObject? GetEventDescription(int factionIndex);

        protected static TextObject? FindTextWithVariation(string textToFind)
        {
            int variationSeparatorIdx = textToFind.LastIndexOf(".");
            if (variationSeparatorIdx > 0)
            {
                var id = textToFind.Substring(0, variationSeparatorIdx);
                var variation = textToFind.Substring(variationSeparatorIdx + 1);
                return GameTexts.FindText(id, variation);
            }
            return GameTexts.FindText(textToFind);
        }

        public override string ToString() => string.Format("{0} - {1}: ({2}->{3};{4}->{5})", _eventDate.ToString(), WarExhaustionType.ToString(), _faction1Value.ToString("F0"), _faction1ExhaustionValue.ToString("F2"), _faction2Value.ToString("F0"), _faction2ExhaustionValue.ToString("F2"));
        public override int GetHashCode() => HashCode.Combine(_eventDate, _faction1ExhaustionValue, _faction2ExhaustionValue);
    }
}