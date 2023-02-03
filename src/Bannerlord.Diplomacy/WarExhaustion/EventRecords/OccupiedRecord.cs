using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Diplomacy.WarExhaustion.EventRecords
{
    internal sealed class OccupiedRecord : WarExhaustionEventRecord
    {
        [SaveableField(60)]
        private readonly Kingdom _faction1;
        [SaveableField(61)]
        private readonly Kingdom _faction2;
        [SaveableField(62)]
        private readonly bool _yieldsDiminishingReturns;

        public override WarExhaustionType WarExhaustionType => WarExhaustionType.Occupied;
        public Kingdom Faction1 => _faction1;
        public Kingdom Faction2 => _faction2;
        public bool YieldsDiminishingReturns => _yieldsDiminishingReturns;

        public OccupiedRecord(CampaignTime eventDate, int faction1Value, float faction1ExhaustionValue, int faction2Value, float faction2ExhaustionValue, Kingdom faction1, Kingdom faction2, bool yieldsDiminishingReturns = false)
            : base(eventDate, faction1Value, faction1ExhaustionValue, faction2Value, faction2ExhaustionValue)
        {
            _faction1 = faction1;
            _faction2 = faction2;
            _yieldsDiminishingReturns = yieldsDiminishingReturns;
        }

        protected override TextObject? GetEventDescription(int factionIndex)
        {
            var (factionEffected, faction, otherFaction) = factionIndex switch
            {
                1 => (Faction1Effected, _faction1, _faction2),
                2 => (Faction2Effected, _faction2, _faction1),
                _ => throw new ArgumentOutOfRangeException(nameof(factionIndex)),
            };

            TextObject? textObject = factionEffected ? GameTexts.FindText("str_war_exhaustion_occupied", _yieldsDiminishingReturns ? "1" : "0") : null;
            if (textObject is not null)
            {
                SetDescriptionVariables(textObject, faction, otherFaction);
            }
            return textObject;
        }

        private void SetDescriptionVariables(TextObject textObject, Kingdom faction, Kingdom otherFaction)
        {
            textObject.SetTextVariable("DATE_STR", _eventDate.ToString());
            textObject.SetTextVariable("FACTION", faction.Name);
            textObject.SetTextVariable("OTHER_FACTION", otherFaction.Name);
        }
    }
}