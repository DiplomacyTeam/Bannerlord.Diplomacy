using Diplomacy.DiplomaticAction.Scoring;

namespace Diplomacy.DiplomaticAction.Alliance
{
    internal sealed class AllianceScoringModel : AbstractPactAllianceScoringModel<AllianceScoringModel>
    {
        public class AllianceScores : IDiplomacyScores
        {
            public int Base => 20;
            public int BelowMedianStrength => 50;
            public int HasCommonEnemy => 20;
            public int HasPotentialEnemy => 20;
            public int TreatiesOverburden => -15;
            public int ExistingAllianceWithEnemy => -1000;
            public int ExistingAllianceWithNeutral => -50;
            public int LeadersRelationship => 30;
            public int PublicRelations => 20;
            public int Tendency => Settings.Instance!.AllianceTendency;
            public int NonAggressionPactWithEnemy => -200;
            public int NonAggressionPactWithNeutral => -10;
            public int NonAggressionPactWithAlly => 5;
            public int AppreciatesTheOffer => 5;
        }
        //Weak, no common or potential enemies, 30 avg relation, no penalties: 20 + 50 + 13.5 + 9 = 92.5
        //Maybe rise up positive scores a bit, or lower expansionism penalty. Right now it's really harsh.

        public override float BaseDiplomaticBarterValue => 50;

        public override IDiplomacyScores Scores => new AllianceScores();
    }
}