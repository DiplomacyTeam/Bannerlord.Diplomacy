using Diplomacy.DiplomaticAction.Conditioning;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Alliance.Conditions
{
    internal sealed class TimeElapsedSinceAllianceFormedCondition : AbstractTimeCondition
    {
        private static readonly TextObject _TTooSoon = new("{=wst0lArp}This alliance hasn't gone on long enough to consider breaking it! It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.");

        protected override TextObject GetFailedConditionText() => _TTooSoon.CopyTextObject();

        protected override bool HasEnoughTimeElapsed(Kingdom kingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType, out float elapsedDaysUntilNow, out int requiredDays)
        {
            requiredDays = Settings.Instance!.MinimumAllianceDuration;
            return !CooldownManager.HasBreakAllianceCooldown(kingdom, otherKingdom, out elapsedDaysUntilNow);
        }
    }
}