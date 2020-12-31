using TaleWorlds.CampaignSystem;

namespace Diplomacy.DiplomaticAction.Alliance
{
    internal class AllianceScoringModel : AbstractScoringModel<AllianceScoringModel>
    {
        public AllianceScoringModel() : base(new AllianceScores()) { }

        public override ExplainedNumber GetScore(Kingdom kingdom, Kingdom otherKingdom, StatExplainer explanation = null)
        {
            return base.GetScore(kingdom, otherKingdom, explanation);
        }

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
