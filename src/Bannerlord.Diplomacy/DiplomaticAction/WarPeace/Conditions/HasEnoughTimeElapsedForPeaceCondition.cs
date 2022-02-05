using Diplomacy.DiplomaticAction.Conditioning;

using TaleWorlds.CampaignSystem;

using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace.Conditions
{
    internal sealed class HasEnoughTimeElapsedForPeaceCondition : AbstractTimeCondition
    {
        private static readonly TextObject _TTooSoon = new("{=ONNcmltF}This war hasn't gone on long enough to consider peace! It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.");

        protected override TextObject GetFailedConditionText() => _TTooSoon.CopyTextObject();

        protected override bool HasEnoughTimeElapsed(Kingdom kingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType, out float elapsedDaysUntilNow, out int requiredDays)
        {
            requiredDays = Settings.Instance!.MinimumWarDurationInDays;
            return CooldownManager.HasExceededMinimumWarDuration(kingdom, otherKingdom, out elapsedDaysUntilNow);
        }
    }
}