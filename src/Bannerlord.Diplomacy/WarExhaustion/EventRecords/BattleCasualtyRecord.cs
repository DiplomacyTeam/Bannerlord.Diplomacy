using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Diplomacy.WarExhaustion.EventRecords
{
    internal sealed class BattleCasualtyRecord : CasualtyRecord
    {
        [SaveableField(25)]
        private readonly TextObject _faction1RelatedPartyName;
        [SaveableField(26)]
        private readonly TextObject _faction2RelatedPartyName;
        [SaveableField(27)]
        private readonly Settlement? _eventRelatedSettlement;

        public TextObject Faction1RelatedPartyName => _faction1RelatedPartyName;
        public TextObject Faction2RelatedPartyName => _faction2RelatedPartyName;
        public Settlement? Settlement => _eventRelatedSettlement;

        public BattleCasualtyRecord(CampaignTime eventDate,
                                    int faction1Value, float faction1ExhaustionValue,
                                    int faction2Value, float faction2ExhaustionValue,
                                    TextObject faction1RelatedPartyName, TextObject faction2RelatedPartyName,
                                    Settlement? eventRelatedSettlement) : base(eventDate, faction1Value, faction1ExhaustionValue, faction2Value, faction2ExhaustionValue)
        {
            _faction1RelatedPartyName = faction1RelatedPartyName;
            _faction2RelatedPartyName = faction2RelatedPartyName;
            _eventRelatedSettlement = eventRelatedSettlement;
        }

        protected override TextObject? GetEventDescription(int factionIndex)
        {
            var (factionValue, factionRelatedPartyName, otherFactionRelatedPartyName) = factionIndex switch
            {
                1 => (_faction1Value, _faction1RelatedPartyName, _faction2RelatedPartyName),
                2 => (_faction2Value, _faction2RelatedPartyName, _faction1RelatedPartyName),
                _ => throw new ArgumentOutOfRangeException(nameof(factionIndex)),
            };

            TextObject? textObject = factionValue != 0 ? GameTexts.FindText("str_war_exhaustion_casualty", _eventRelatedSettlement is not null ? "2" : "1") : null;
            if (textObject is not null)
            {
                SetDescriptionVariables(textObject, factionValue, factionRelatedPartyName, otherFactionRelatedPartyName);
            }
            return textObject;
        }

        private void SetDescriptionVariables(TextObject textObject, float factionValue, TextObject factionRelatedPartyName, TextObject otherFactionRelatedPartyName)
        {
            textObject.SetTextVariable("DATE_STR", _eventDate.ToString());
            textObject.SetTextVariable("FACTION_PARTY", factionRelatedPartyName);
            textObject.SetTextVariable("OTHER_FACTION_PARTY", otherFactionRelatedPartyName);
            if (_eventRelatedSettlement is not null)
                textObject.SetTextVariable("SETTLEMENT", _eventRelatedSettlement.Name);

            SetDescriptionVariables(textObject, factionValue);
        }
    }
}