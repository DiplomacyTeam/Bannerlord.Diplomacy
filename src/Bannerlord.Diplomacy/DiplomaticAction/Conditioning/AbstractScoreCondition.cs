using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Conditioning
{
    internal abstract class AbstractScoreCondition : AbstractDiplomacyCondition
    {
        internal override DiplomacyConditionType ConditionType => DiplomacyConditionType.ScoreRelated;

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            textObject = null;
            var scoreTooLow = GetActionScore(kingdom, otherKingdom, kingdomPartyType) < GetPassThreshold();
            if (scoreTooLow)
            {
                var scoreTooLowText = GetFailedConditionText();
                textObject = scoreTooLowText;
            }
            return !scoreTooLow;
        }

        protected abstract float GetActionScore(Kingdom kingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType);
        protected abstract float GetPassThreshold();
        protected abstract TextObject GetFailedConditionText();
    }
}
