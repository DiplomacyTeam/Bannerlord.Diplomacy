namespace Diplomacy.DiplomaticAction.Alliance
{
    internal sealed class AllianceScoringModel : AbstractScoringModel<AllianceScoringModel>
    {
        public AllianceScoringModel() : base(new AllianceScores()) { }

        public class AllianceScores : IDiplomacyScores
        {
            public int Base => 20;

            public int BelowMedianStrength => 50;

            public int HasCommonEnemy => 50;

            public int ExistingAllianceWithEnemy => -1000;

            public int ExistingAllianceWithNeutral => -50;

            public int Relationship => 50;

            public int Tendency => Settings.Instance!.AllianceTendency;

            public int NonAggressionPactWithEnemy => -1000;

            public int NonAggressionPactWithNeutral => -10;

            public int LeaderClanMarriage => 50;
        }
    }
}