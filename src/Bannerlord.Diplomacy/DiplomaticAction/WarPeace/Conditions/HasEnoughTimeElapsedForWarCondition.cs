using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace.Conditions
{
    internal class HasEnoughTimeElapsedForWarCondition : IDiplomacyCondition
    {
        private static readonly TextObject _TTooSoon = new("{=GuPOCNNZ}The truce was concluded too recently to consider breaking it now! It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.");

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            textObject = null;
            var hasEnoughTimeElapsed = !CooldownManager.HasDeclareWarCooldown(kingdom, otherKingdom, out var elapsedDaysUntilNow);
            if (!hasEnoughTimeElapsed)
            {
                textObject = _TTooSoon.CopyTextObject();
                textObject.SetTextVariable("ELAPSED_DAYS", (float) Math.Floor(elapsedDaysUntilNow));
                textObject.SetTextVariable("REQUIRED_DAYS", Settings.Instance!.DeclareWarCooldownInDays);
            }
            return hasEnoughTimeElapsed;
        }
    }
}