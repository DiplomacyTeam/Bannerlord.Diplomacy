using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace.Conditions
{
    class HasLowWarExhaustionCondition : IDiplomacyCondition
    {
        private const string WAR_EXHAUSTION_TOO_HIGH = "{=QVp4v2MG}War exhaustion is too high to declare war. Current war exhaustion is {CURRENT_WAR_EXHAUSTION} and {LOW_WAR_EXHAUSTION_THRESHOLD} is the highest allowed.";

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            textObject = null;
            var hasDeclareWarCooldown = CooldownManager.HasDeclareWarCooldown(kingdom, otherKingdom, out var elapsedTime);
            var hasLowWarExhaustion = true;
            if (Settings.Instance!.EnableWarExhaustion)
            {
                hasLowWarExhaustion = WarExhaustionManager.Instance.HasLowWarExhaustion(kingdom, otherKingdom);
            }

            if (!hasLowWarExhaustion)
            {
                var lowWarExhaustionThreshold = (int)WarExhaustionManager.GetLowWarExhaustion();
                textObject = new TextObject(WAR_EXHAUSTION_TOO_HIGH);
                textObject.SetTextVariable("LOW_WAR_EXHAUSTION_THRESHOLD", lowWarExhaustionThreshold);
                textObject.SetTextVariable("CURRENT_WAR_EXHAUSTION", (int)Math.Ceiling(WarExhaustionManager.Instance.GetWarExhaustion(kingdom, otherKingdom)));
            }

            return hasLowWarExhaustion;
        }
    }
}
