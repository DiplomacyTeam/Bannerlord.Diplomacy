using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Diplomacy.WarExhaustion.EventRecords
{
    internal sealed class CaravanRaidRecord : WarExhaustionEventRecord
    {
        [SaveableField(45)]
        private readonly TextObject _faction1RelatedPartyName;
        [SaveableField(46)]
        private readonly TextObject _faction2RelatedPartyName;

        public override WarExhaustionType WarExhaustionType => WarExhaustionType.CaravanRaid;
        public TextObject Faction1RelatedPartyName => _faction1RelatedPartyName;
        public TextObject Faction2RelatedPartyName => _faction2RelatedPartyName;

        public CaravanRaidRecord(CampaignTime eventDate,
                                 int faction1Value, float faction1ExhaustionValue,
                                 int faction2Value, float faction2ExhaustionValue,
                                 TextObject caravanPartyName, TextObject raidingPartyName) : base(eventDate, faction1Value, faction1ExhaustionValue, faction2Value, faction2ExhaustionValue)
        {
            _faction1RelatedPartyName = caravanPartyName;
            _faction2RelatedPartyName = raidingPartyName;
        }

        protected override TextObject? GetEventDescription(int factionIndex)
        {
            var (factionEffected, factionRelatedPartyName, otherFactionRelatedPartyName) = factionIndex switch
            {
                1 => (Faction1Effected, _faction1RelatedPartyName, _faction2RelatedPartyName),
                2 => (Faction2Effected, _faction2RelatedPartyName, _faction1RelatedPartyName),
                _ => throw new ArgumentOutOfRangeException(nameof(factionIndex)),
            };

            TextObject? textObject = factionEffected ? GameTexts.FindText("str_war_exhaustion_caravan_raid") : null;
            if (textObject is not null)
            {
                SetDescriptionVariables(textObject, factionRelatedPartyName, otherFactionRelatedPartyName);
            }
            return textObject;
        }

        private void SetDescriptionVariables(TextObject textObject, TextObject factionRelatedPartyName, TextObject otherFactionRelatedPartyName)
        {
            textObject.SetTextVariable("DATE_STR", _eventDate.ToString());
            textObject.SetTextVariable("RAIDED_CARAVAN", factionRelatedPartyName);
            textObject.SetTextVariable("OTHER_FACTION_PARTY", otherFactionRelatedPartyName);
        }
    }
}