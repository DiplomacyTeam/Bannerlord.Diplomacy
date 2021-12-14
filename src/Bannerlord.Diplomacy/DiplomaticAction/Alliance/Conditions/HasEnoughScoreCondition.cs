using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Alliance.Conditions
{
    internal sealed class HasEnoughScoreCondition : AbstractScoreCondition
    {
        private static readonly TextObject FailedConditionText = new("{=VvTTrRpl}This faction is not interested in forming an alliance with you.");

        protected override float GetActionScore(Kingdom kingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType) =>
            DeclareAllianceAction.GetActionScore(kingdom, otherKingdom, kingdomPartyType);
        //!AllianceScoringModel.Instance.IsAcceptancePossible(kingdom, otherKingdom);

        protected override float GetPassThreshold() => - AllianceScoringModel.Instance.ScoreThreshold; //this is auto-rejection threshold!
        protected override TextObject GetFailedConditionText() => FailedConditionText;
    }
}