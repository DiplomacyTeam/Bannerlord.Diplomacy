using Bannerlord.ButterLib.Common.Helpers;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Diplomacy.WarExhaustion.EventRecords
{
    internal sealed class SiegeRecord : WarExhaustionEventRecord
    {
        [SaveableField(30)]
        private readonly Settlement _eventRelatedSettlement;
        [SaveableField(31)]
        private readonly TextObject _faction1RelatedPartyName;
        [SaveableField(32)]
        private readonly TextObject _faction2RelatedPartyName;
        [SaveableField(33)]
        private readonly bool _yieldsDiminishingReturns;

        public override WarExhaustionType WarExhaustionType => WarExhaustionType.Siege;
        public Settlement Settlement => _eventRelatedSettlement;
        public TextObject Faction1RelatedPartyName => _faction1RelatedPartyName;
        public TextObject Faction2RelatedPartyName => _faction2RelatedPartyName;
        public bool YieldsDiminishingReturns => _yieldsDiminishingReturns;

        public SiegeRecord(CampaignTime eventDate, Settlement eventRelatedSettlement,
                           int faction1Value, float faction1ExhaustionValue,
                           int faction2Value, float faction2ExhaustionValue,
                           TextObject faction1RelatedPartyName, TextObject faction2RelatedPartyName,
                           bool yieldsDiminishingReturns = false) : base(eventDate, faction1Value, faction1ExhaustionValue, faction2Value, faction2ExhaustionValue)
        {
            _eventRelatedSettlement = eventRelatedSettlement;
            _faction1RelatedPartyName = faction1RelatedPartyName;
            _faction2RelatedPartyName = faction2RelatedPartyName;
            _yieldsDiminishingReturns = yieldsDiminishingReturns;
        }

        protected override TextObject? GetEventDescription(int factionIndex)
        {
            var (factionEffected, factionRelatedPartyName, otherFactionRelatedPartyName) = factionIndex switch
            {
                1 => (Faction1Effected, _faction1RelatedPartyName, _faction2RelatedPartyName),
                2 => (Faction2Effected, _faction2RelatedPartyName, _faction1RelatedPartyName),
                _ => throw new ArgumentOutOfRangeException(nameof(factionIndex)),
            };

            TextObject? textObject = factionEffected ? GameTexts.FindText("str_war_exhaustion_siege", _yieldsDiminishingReturns ? "1" : "0") : null;
            if (textObject is not null)
            {
                SetDescriptionVariables(textObject, factionRelatedPartyName, otherFactionRelatedPartyName);
            }
            return textObject;
        }

        private void SetDescriptionVariables(TextObject textObject, TextObject factionRelatedPartyName, TextObject otherFactionRelatedPartyName)
        {
            textObject.SetTextVariable("DATE_STR", _eventDate.ToString());
            textObject.SetTextVariable("FACTION_PARTY", factionRelatedPartyName);
            textObject.SetTextVariable("OTHER_FACTION_PARTY", otherFactionRelatedPartyName);
            LocalizationHelper.SetEntityProperties(textObject, "SETTLEMENT", _eventRelatedSettlement);
        }
    }
}