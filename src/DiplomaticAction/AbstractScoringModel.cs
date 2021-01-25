using Diplomacy.Extensions;

using System;
using System.Drawing.Text;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction
{
    internal abstract class AbstractScoringModel<T> where T : AbstractScoringModel<T>, new()
    {
        public static T Instance { get; } = new T();

        public virtual float ScoreThreshold { get; } = 100.0f;

        protected IScores Scores { get; init; }

        protected AbstractScoringModel(IScores scores) => Scores = scores;

        public virtual ExplainedNumber GetScore(Kingdom ourKingdom, Kingdom otherKingdom, bool includeDesc = false)
        {
            var explainedNum = new ExplainedNumber(Scores.Base, includeDesc);

            TextObject? CreateTextWithKingdom(string text, Kingdom kingdom) => includeDesc
                ? new TextObject(text).SetTextVariable("KINGDOM", kingdom.Name)
                : null;

            /// Weak Kingdom (Us)

            if (!ourKingdom.IsStrong())
                explainedNum.Add(Scores.BelowMedianStrength, _txtWeakKingdom);

            /// Common Enemies
            
            var commonEnemies = FactionManager.GetEnemyKingdoms(ourKingdom).Intersect(FactionManager.GetEnemyKingdoms(otherKingdom));

            foreach (var commonEnemy in commonEnemies)
                explainedNum.Add(Scores.HasCommonEnemy, CreateTextWithKingdom(_strCommonEnemy, commonEnemy));

            /// Their Alliances with Enemies

            var alliedEnemies = Kingdom.All
                .Where(k => k != ourKingdom
                         && k != otherKingdom
                         && FactionManager.IsAlliedWithFaction(otherKingdom, k)
                         && FactionManager.IsAtWarAgainstFaction(ourKingdom, k));

            foreach (var alliedEnemy in alliedEnemies)
                explainedNum.Add(Scores.ExistingAllianceWithEnemy, CreateTextWithKingdom(_strAlliedToEnemy, alliedEnemy));

            /// Their Alliances with Neutrals

            var alliedNeutrals = Kingdom.All
                .Where(k => k != ourKingdom
                         && k != otherKingdom
                         && FactionManager.IsAlliedWithFaction(otherKingdom, k)
                         && !FactionManager.IsAtWarAgainstFaction(ourKingdom, k));

            // FIXME: alliedNeutrals also includes common allies as it's coded... Should they be scored differently? Probable answer: YES!

            foreach (var alliedNeutral in alliedNeutrals)
                explainedNum.Add(Scores.ExistingAllianceWithNeutral, CreateTextWithKingdom(_strAlliedToNeutral, alliedNeutral));

            /// Relationship

            var relationMult = MBMath.ClampFloat((float)Math.Log((ourKingdom.Leader.GetRelation(otherKingdom.Leader) + 100f) / 100f, 1.5),
                                                 -1f,
                                                 +1f);

            explainedNum.Add(Scores.Relationship * relationMult, _txtRelationship);

            /// Expansionism (Them)
            
            var expansionismPenalty = otherKingdom.GetExpansionismDiplomaticPenalty();

            if (expansionismPenalty < 0)
                explainedNum.Add(expansionismPenalty, _txtExpansionism);

            return explainedNum;
        }

        public virtual bool ShouldFormBidirectional(Kingdom ourKingdom, Kingdom otherKingdom)
            => ShouldForm(ourKingdom, otherKingdom) && ShouldForm(otherKingdom, ourKingdom);

        public virtual bool ShouldForm(Kingdom ourKingdom, Kingdom otherKingdom)
            => GetScore(ourKingdom, otherKingdom).ResultNumber >= ScoreThreshold;

        public interface IScores
        {
            public int Base { get; }

            public int BelowMedianStrength { get; }

            public int HasCommonEnemy { get; }

            public int ExistingAllianceWithEnemy { get; }

            public int ExistingAllianceWithNeutral { get; }

            public int Relationship { get; }
        }

        private static readonly TextObject _txtWeakKingdom = new("{=q5qphBwi}Weak Kingdom");
        private static readonly TextObject _txtRelationship = new("{=sygtLRqA}Relationship");
        private static readonly TextObject _txtExpansionism = new("{=CxdpR6w4}Expansionism");

        private const string _strWarWithKingdom = "{=RqQ4oqvl}War with {KINGDOM}";
        private const string _strAllianceWithKingdom = "{=cmOSpfyW}Alliance with {KINGDOM}";

        private const string _strCommonEnemy = _strWarWithKingdom;
        private const string _strAlliedToEnemy = _strAllianceWithKingdom;
        private const string _strAlliedToNeutral = _strAllianceWithKingdom;
    }
}