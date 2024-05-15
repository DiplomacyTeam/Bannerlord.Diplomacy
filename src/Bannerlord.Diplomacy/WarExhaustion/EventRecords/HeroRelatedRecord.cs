using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Diplomacy.WarExhaustion.EventRecords
{
    internal abstract class HeroRelatedRecord : WarExhaustionEventRecord
    {
        [SaveableField(50)]
        protected readonly Hero _hero;
        [SaveableField(51)]
        protected readonly TextObject _otherPartyName;
        [SaveableField(52)]
        protected readonly bool _yieldsDiminishingReturns;

        public Hero Hero => _hero;
        public TextObject OtherPartyName => _otherPartyName;
        public bool YieldsDiminishingReturns => _yieldsDiminishingReturns;

        protected HeroRelatedRecord(CampaignTime eventDate, Hero hero, int faction1Value, float faction1ExhaustionValue, int faction2Value, float faction2ExhaustionValue, TextObject otherPartyName, bool yieldsDiminishingReturns = false)
            : base(eventDate, faction1Value, faction1ExhaustionValue, faction2Value, faction2ExhaustionValue)
        {
            _hero = hero;
            _otherPartyName = otherPartyName;
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

            TextObject? textObject = factionEffected ? GetDescriptionTextObject() : null;
            if (textObject is not null)
            {
                SetDescriptionVariables(textObject);
            }
            return textObject;
        }

        protected abstract TextObject? GetDescriptionTextObject();

        protected virtual void SetDescriptionVariables(TextObject textObject)
        {
            textObject.SetTextVariable("DATE_STR", _eventDate.ToString());
            textObject.SetTextVariable("HERO", _hero.Name);
            textObject.SetTextVariable("OTHER_FACTION_PARTY", _otherPartyName);
        }
    }
}