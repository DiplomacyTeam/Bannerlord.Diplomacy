using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Diplomacy.WarExhaustion.EventRecords
{
    internal sealed class RaidRecord : WarExhaustionEventRecord
    {
        [SaveableField(40)]
        private readonly Village _raidedVillage;
        [SaveableField(41)]
        private readonly TextObject _raidingPartyName;
        [SaveableField(42)]
        private readonly bool _yieldsDiminishingReturns;

        public override WarExhaustionType WarExhaustionType => WarExhaustionType.Raid;
        public Village RaidedVillage => _raidedVillage;
        public TextObject RaidingPartyName => _raidingPartyName;
        public bool YieldsDiminishingReturns => _yieldsDiminishingReturns;

        public RaidRecord(CampaignTime eventDate, Village raidedVillage,
                          int faction1Value, float faction1ExhaustionValue,
                          int faction2Value, float faction2ExhaustionValue,
                          TextObject raidingPartyName, bool yieldsDiminishingReturns = false) : base(eventDate, faction1Value, faction1ExhaustionValue, faction2Value, faction2ExhaustionValue)
        {
            _raidedVillage = raidedVillage;
            _raidingPartyName = raidingPartyName;
            _yieldsDiminishingReturns = yieldsDiminishingReturns;
        }

        protected override TextObject? GetEventDescription(int factionIndex)
        {
            var factionEffected = factionIndex switch
            {
                1 => Faction1Effected,
                2 => Faction2Effected,
                _ => throw new ArgumentOutOfRangeException(nameof(factionIndex)),
            };

            TextObject? textObject = factionEffected ? GameTexts.FindText("str_war_exhaustion_raid", _yieldsDiminishingReturns ? "1" : "0") : null;
            if (textObject is not null)
            {
                SetDescriptionVariables(textObject);
            }
            return textObject;
        }

        private void SetDescriptionVariables(TextObject textObject)
        {
            textObject.SetTextVariable("DATE_STR", _eventDate.ToString());
            textObject.SetTextVariable("RAIDED_VILLAGE", _raidedVillage.Name);
            textObject.SetTextVariable("OTHER_FACTION_PARTY", _raidingPartyName);
        }
    }
}