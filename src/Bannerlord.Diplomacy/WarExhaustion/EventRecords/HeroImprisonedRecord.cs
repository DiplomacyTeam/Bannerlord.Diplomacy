using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.WarExhaustion.EventRecords
{
    internal sealed class HeroImprisonedRecord : HeroRelatedRecord
    {
        public override WarExhaustionType WarExhaustionType => WarExhaustionType.HeroImprisoned;

        public HeroImprisonedRecord(CampaignTime eventDate, Hero hero, int faction1Value, float faction1ExhaustionValue, int faction2Value, float faction2ExhaustionValue, TextObject otherPartyName, bool yieldsDiminishingReturns = false)
            : base(eventDate, hero, faction1Value, faction1ExhaustionValue, faction2Value, faction2ExhaustionValue, otherPartyName, yieldsDiminishingReturns) { }

        protected override TextObject? GetDescriptionTextObject()
        {
            return GameTexts.FindText("str_war_exhaustion_hero_imprisoned", _yieldsDiminishingReturns ? "1" : "0");
        }
    }
}