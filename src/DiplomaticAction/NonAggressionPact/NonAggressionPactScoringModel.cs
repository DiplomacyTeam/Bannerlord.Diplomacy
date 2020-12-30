using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.DiplomaticAction.NonAggressionPact
{
    internal class NonAggressionPactScoringModel : AbstractScoringModel<NonAggressionPactScoringModel>
    {
        public NonAggressionPactScoringModel() : base(new NonAggressionScores()) { }

        public override ExplainedNumber GetScore(Kingdom kingdom, Kingdom otherKingdom, StatExplainer explanation = null)
        {
            return base.GetScore(kingdom, otherKingdom, explanation);
        }

        public class NonAggressionScores : IScores
        {
            public int Base => 50;
            public int BelowMedianStrength => 50;
            public int HasCommonEnemy => 50;
            public int ExistingAllianceWithEnemy => -1000;
            public int ExistingAllianceWithNeutral => -50;
            public int Relationship => 50;
        }
    }
}
