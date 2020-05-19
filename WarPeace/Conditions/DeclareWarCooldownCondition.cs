using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.WarPeace.Conditions
{
    class DeclareWarCooldownCondition : IDiplomacyCondition
    {
        private const string DECLARE_WAR_COOLDOWN = "{=jPHYDjXQ}Cannot declare war so soon after making peace! It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.";

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            bool hasDeclareWarCooldown = CooldownManager.HasDeclareWarCooldown(kingdom, otherKingdom, out float elapsedTime);
            if (hasDeclareWarCooldown)
            {
                int declareWarCooldownDuration = Settings.Instance.DeclareWarCooldownInDays;

                textObject = new TextObject(DECLARE_WAR_COOLDOWN);
                textObject.SetTextVariable("ELAPSED_DAYS", (float)Math.Floor(elapsedTime));
                textObject.SetTextVariable("REQUIRED_DAYS", declareWarCooldownDuration);
            }
            return !hasDeclareWarCooldown;
        }
    }
}
