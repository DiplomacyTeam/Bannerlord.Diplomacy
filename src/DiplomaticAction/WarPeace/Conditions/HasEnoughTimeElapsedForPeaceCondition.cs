using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace.Conditions
{
    class HasEnoughTimeElapsedForPeaceCondition : IDiplomacyCondition
    {
        private const string TOO_SOON = "{=ONNcmltF}This war hasn't gone on long enough to consider peace! It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.";

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            textObject = null;
            var hasEnoughTimeElapsed = CooldownManager.HasExceededMinimumWarDuration(kingdom, otherKingdom, out var elapsedDaysUntilNow);
            if (!hasEnoughTimeElapsed)
            {
                textObject = new TextObject(TOO_SOON);
                textObject.SetTextVariable("ELAPSED_DAYS", (float)Math.Floor(elapsedDaysUntilNow));
                textObject.SetTextVariable("REQUIRED_DAYS", Settings.Instance.MinimumWarDurationInDays);
            }
            return hasEnoughTimeElapsed;
        }
    }
}
