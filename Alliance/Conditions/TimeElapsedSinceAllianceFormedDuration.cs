using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.Alliance.Conditions
{
    class TimeElapsedSinceAllianceFormedCondition : IDiplomacyCondition
    {
        private const string TOO_SOON = "{=wst0lArp}This alliance hasn't gone on long enough to consider breaking it! It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.";

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            bool hasEnoughTimeElapsed = !CooldownManager.HasBreakAllianceCooldown(kingdom, otherKingdom, out float elapsedDaysUntilNow);
            if (!hasEnoughTimeElapsed)
            {
                textObject = new TextObject(TOO_SOON);
                textObject.SetTextVariable("ELAPSED_DAYS", (float)Math.Floor(elapsedDaysUntilNow));
                textObject.SetTextVariable("REQUIRED_DAYS", Settings.Instance.MinimumAllianceDuration);
            }

            return hasEnoughTimeElapsed;
        }
    }
}
