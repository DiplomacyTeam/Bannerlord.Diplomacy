using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction
{
    internal abstract class AbstractTimeCondition : AbstractDiplomacyCondition
    {
        internal override DiplomacyConditionType ConditionType => DiplomacyConditionType.TimeRelated;

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            textObject = null;
            var hasEnoughTimeElapsed = HasEnoughTimeElapsed(kingdom, otherKingdom, kingdomPartyType, out var elapsedDaysUntilNow, out var requiredDays);
            if (!hasEnoughTimeElapsed)
            {
                textObject = GetFailedConditionText();
                textObject.SetTextVariable("ELAPSED_DAYS", (int)Math.Floor(elapsedDaysUntilNow));
                textObject.SetTextVariable("REQUIRED_DAYS", requiredDays);
            }
            return hasEnoughTimeElapsed;
        }

        protected abstract bool HasEnoughTimeElapsed(Kingdom kingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType, out float elapsedDaysUntilNow, out int requiredDays);
        protected abstract TextObject GetFailedConditionText();
    }
}
