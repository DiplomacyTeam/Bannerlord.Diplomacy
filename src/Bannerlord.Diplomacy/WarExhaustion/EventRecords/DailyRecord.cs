using Bannerlord.ButterLib.Common.Helpers;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Diplomacy.WarExhaustion.EventRecords
{
    internal sealed class DailyRecord : WarExhaustionEventRecord
    {
        [SaveableField(10)]
        private readonly bool _faction1IsOccupied;
        [SaveableField(11)]
        private readonly bool _faction2IsOccupied;

        public override WarExhaustionType WarExhaustionType => WarExhaustionType.Daily;
        public int Faction1Days => _faction1Value;
        public int Faction2Days => _faction2Value;
        public bool Faction1IsOccupied => _faction1IsOccupied;
        public bool Faction2IsOccupied => _faction2IsOccupied;
        public float Faction1DailyExhaustionGrowth => _faction1ExhaustionValue / Math.Max(_faction1Value, 1f);
        public float Faction2DailyExhaustionGrowth => _faction2ExhaustionValue / Math.Max(_faction2Value, 1f);

        public DailyRecord(CampaignTime eventDate,
                           int faction1Value, float faction1ExhaustionValue,
                           int faction2Value, float faction2ExhaustionValue,
                           bool faction1IsOccupied = false, bool faction2IsOccupied = false) : base(eventDate, faction1Value, faction1ExhaustionValue, faction2Value, faction2ExhaustionValue)
        {
            _faction1IsOccupied = faction1IsOccupied;
            _faction2IsOccupied = faction2IsOccupied;
        }

        protected override TextObject? GetEventDescription(int factionIndex)
        {
            var (factionValue, factionExhaustionValue, factionIsOccupied) = factionIndex switch
            {
                1 => (_faction1Value, _faction1ExhaustionValue, _faction1IsOccupied),
                2 => (_faction2Value, _faction2ExhaustionValue, _faction2IsOccupied),
                _ => throw new ArgumentOutOfRangeException(nameof(factionIndex)),
            };

            TextObject? textObject = factionValue != 0 ? GameTexts.FindText("str_war_exhaustion_daily", factionIsOccupied ? "1" : "0") : null;
            if (textObject is not null)
            {
                SetDescriptionVariables(textObject, factionValue, factionExhaustionValue);
            }
            return textObject;
        }

        private void SetDescriptionVariables(TextObject textObject, float factionValue, float factionExhaustionValue)
        {
            var endDate = _eventDate + CampaignTime.Days(factionValue) - CampaignTime.Minutes(1);
            var factionDailyExhaustionGrowth = factionExhaustionValue / Math.Max(factionValue, 1f);

            textObject.SetTextVariable("START_DATE", _eventDate.ToString());
            textObject.SetTextVariable("END_DATE", endDate.ToString());
            textObject.SetTextVariable("PER_DAY_VALUE", factionDailyExhaustionGrowth.ToString("F"));
            LocalizationHelper.SetNumericVariable(textObject, "VALUE", factionValue);
        }

        internal bool CanBeCompoundedWith(DailyRecord otherDailyRecord) => _faction1IsOccupied == otherDailyRecord.Faction1IsOccupied
                                                                           && _faction2IsOccupied == otherDailyRecord.Faction2IsOccupied
                                                                           && Faction1DailyExhaustionGrowth == otherDailyRecord.Faction1DailyExhaustionGrowth
                                                                           && Faction2DailyExhaustionGrowth == otherDailyRecord.Faction2DailyExhaustionGrowth;

        internal DailyRecord CompoundWith(DailyRecord otherDailyRecord) => new(_eventDate,
                                                                               _faction1Value + otherDailyRecord.Faction1Value, _faction1ExhaustionValue + otherDailyRecord.Faction1ExhaustionValue,
                                                                               _faction2Value + otherDailyRecord.Faction2Value, _faction2ExhaustionValue + otherDailyRecord.Faction2ExhaustionValue,
                                                                               _faction1IsOccupied, _faction2IsOccupied);
    }
}