namespace Diplomacy.DiplomaticAction.Alliance
{
    internal sealed class AllianceScoringModel : AbstractScoringModel<AllianceScoringModel>
    {
        public AllianceScoringModel() : base(new AllianceScores()) { }

        public class AllianceScores : IScores
        {
            public int Base => 0;

            public int BelowMedianStrength => 50;

            public int HasCommonEnemy => 50;

            public int ExistingAllianceWithEnemy => -1000;

            public int ExistingAllianceWithNeutral => -50;

            public int Relationship => 50;
        }
    }
}
