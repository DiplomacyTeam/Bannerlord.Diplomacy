namespace Diplomacy.DiplomaticAction.NonAggressionPact
{
    internal sealed class NonAggressionPactScoringModel : AbstractScoringModel<NonAggressionPactScoringModel>
    {
        public NonAggressionPactScoringModel() : base(new NonAggressionPactScores()) { }

        public class NonAggressionPactScores : IDiplomacyScores
        {
            public int Base => 30;
            public int BelowMedianStrength => 50;
            public int HasCommonEnemy => 15;
            public int HasPotentialEnemy => 0; //does not apply to NAPs
            public int TooManyTreaties => -15;
            public int ExistingAllianceWithEnemy => -1000;
            public int ExistingAllianceWithNeutral => -50;
            public int LeadersRelationship => 30;
            public int PublicRelations => 20;
            public int Tendency => Settings.Instance!.NonAggressionPactTendency;
            public int NonAggressionPactWithEnemy => -20;
            public int NonAggressionPactWithNeutral => -10;
            public int NonAggressionPactWithAlly => 10;
            public int AppreciatesTheOffer => 5;
        }
        //Weak, no common enemies, 30 avg relation, no penalties: 30 + 50 + 13.5 + 9 = 102.5
        //Lower relations will require common enemies.
        //Maybe rise up positive scores a bit, or lower expansionism penalty. Right now it's really harsh.
    }
}
