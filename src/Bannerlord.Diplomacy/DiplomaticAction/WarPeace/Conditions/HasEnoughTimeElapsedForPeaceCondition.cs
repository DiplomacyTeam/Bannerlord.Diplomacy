using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace.Conditions
{
    internal class HasEnoughTimeElapsedForPeaceCondition : IDiplomacyCondition
    {
        private static readonly TextObject _TTooSoon = new("{=ONNcmltF}This war hasn't gone on long enough to consider peace! It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.");

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            textObject = null;
            var hasEnoughTimeElapsed = CooldownManager.HasExceededMinimumWarDuration(kingdom, otherKingdom, out var elapsedDaysUntilNow);
            if (!hasEnoughTimeElapsed)
            {
                textObject = _TTooSoon.CopyTextObject();
                textObject.SetTextVariable("ELAPSED_DAYS", (float) Math.Floor(elapsedDaysUntilNow));
                textObject.SetTextVariable("REQUIRED_DAYS", Settings.Instance!.MinimumWarDurationInDays);
            }
            return hasEnoughTimeElapsed;
        }
    }
}