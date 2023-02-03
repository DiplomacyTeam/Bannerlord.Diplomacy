using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Diplomacy.WarExhaustion.EventRecords
{
    internal sealed class HeroPerishedRecord : HeroRelatedRecord
    {
        [SaveableField(56)]
        private readonly KillCharacterAction.KillCharacterActionDetail _deathDetail;

        public override WarExhaustionType WarExhaustionType => WarExhaustionType.HeroPerished;

        public KillCharacterAction.KillCharacterActionDetail DeathDetail => _deathDetail;

        public HeroPerishedRecord(CampaignTime eventDate, Hero hero, int faction1Value, float faction1ExhaustionValue, int faction2Value, float faction2ExhaustionValue, TextObject otherPartyName, KillCharacterAction.KillCharacterActionDetail deathDetail, bool yieldsDiminishingReturns = false)
            : base(eventDate, hero, faction1Value, faction1ExhaustionValue, faction2Value, faction2ExhaustionValue, otherPartyName, yieldsDiminishingReturns)
        {
            _deathDetail = deathDetail;
        }

        protected override TextObject? GetDescriptionTextObject()
        {
            return GameTexts.FindText("str_war_exhaustion_hero_perished", ((int) _deathDetail).ToString());
        }

        protected override void SetDescriptionVariables(TextObject textObject)
        {
            base.SetDescriptionVariables(textObject);
            textObject.SetTextVariable("YIELDS_DIMINISHING_RETURNS", _yieldsDiminishingReturns ? 1 : 0);
        }
    }
}