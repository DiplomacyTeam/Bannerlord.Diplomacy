using Diplomacy.DiplomaticAction.Conditioning;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.NonAggressionPact
{
    internal sealed class HasEnoughScoreCondition : AbstractScoreCondition
    {
        private static readonly TextObject FailedConditionText = new("{=M4SGjzQP}This faction is not interested in forming a non-aggression pact with you.");

        protected override float GetActionScore(Kingdom kingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType) =>
            FormNonAggressionPactAction.GetActionScore(kingdom, otherKingdom, kingdomPartyType);

        protected override float GetPassThreshold() => NonAggressionPactScoringModel.ConsiderationThreshold;

        protected override TextObject GetFailedConditionText() => FailedConditionText;
    }
}