namespace Diplomacy.DiplomaticAction.NonAggressionPact
{
    internal sealed class NonAggressionPactScoringModel : AbstractScoringModel<NonAggressionPactScoringModel>
    {
        public NonAggressionPactScoringModel() : base(new NonAggressionPactScores()) { }

        public class NonAggressionPactScores : IDiplomacyScores
        {
            public int Base => 50;
            public int BelowMedianStrength => 50;
            public int HasCommonEnemy => 50;
            public int ExistingAllianceWithEnemy => -1000;
            public int ExistingAllianceWithNeutral => -50;
            public int Relationship => 50;
            public int Tendency => Settings.Instance!.NonAggressionPactTendency;
            public int NonAggressionPactWithEnemy => -20;
            public int NonAggressionPactWithNeutral => -10;
            public int LeaderClanMarriage => 50;
        }
    }
}