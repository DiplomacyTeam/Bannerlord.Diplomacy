using System.Collections.Generic;

using TaleWorlds.CampaignSystem;

namespace Diplomacy.DiplomaticAction.Scoring
{
    public abstract class AbstractAgreementScoringModel<T> where T : AbstractAgreementScoringModel<T>, new()
    {
        public static T Instance { get; } = new();

        public const float ProactiveBarterThreshold = 110.0f;

        public const float AcceptOrProposeThreshold = 100.0f;

        public const float ConsiderationThreshold = -10.0f;

        public abstract float BaseDiplomaticBarterValue { get; }

        public abstract List<ScoreEvaluator> ScoreEvaluators { get; }
        public abstract float BaseScore { get; }

        public ExplainedNumber GetScore(Kingdom ourKingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType, bool includeDesc = false)
        {
            ExplainedNumber explainedNumber = new ExplainedNumber(BaseScore, includeDesc);
            foreach (var evaluateScoreAction in ScoreEvaluators) evaluateScoreAction(ourKingdom, otherKingdom, kingdomPartyType, ref explainedNumber);
            return explainedNumber;
        }

        public bool IsAcceptancePossible(Kingdom kingdom, Kingdom otherKingdom)
        {
            return GetScore(otherKingdom, kingdom, DiplomaticPartyType.Proposer).ResultNumber >= ConsiderationThreshold;
        }

        public float GetDiplomaticBarterCost(Kingdom kingdom, Kingdom otherKingdom)
        {
            return (AcceptOrProposeThreshold - GetScore(otherKingdom, kingdom, DiplomaticPartyType.Proposer).ResultNumber) / AcceptOrProposeThreshold * BaseDiplomaticBarterValue;
        }

        public delegate void ScoreEvaluator(Kingdom ourKingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType, ref ExplainedNumber explainedNumber);
    }
}