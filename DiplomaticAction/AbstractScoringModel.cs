using DiplomacyFixes.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction
{
    public abstract class AbstractScoringModel<T> where T : AbstractScoringModel<T>, new()
    {
        protected AbstractScoringModel(IScores scores)
        {
            this.Scores = scores;
        }

        public static T Instance { get; } = new T();

        public virtual float ScoreThreshold { get; } = 100.0f;
        protected IScores Scores { get; set; }

        private static readonly TextObject _weakFaction = new TextObject("{=q5qphBwi}Weak Faction");
        private static readonly TextObject _relationship = new TextObject("{=sygtLRqA}Relationship");

        public virtual ExplainedNumber GetScore(Kingdom kingdom, Kingdom otherKingdom, StatExplainer explanation = null)
        {
            ExplainedNumber explainedNumber = new ExplainedNumber(Scores.Base, explanation, null);

            // weak faction bonus
            if (!kingdom.IsStrong())
            {
                explainedNumber.Add(Scores.BelowMedianStrength, _weakFaction);
            }

            // common enemies
            IEnumerable<Kingdom> commonEnemies = FactionManager.GetEnemyKingdoms(kingdom).Intersect(FactionManager.GetEnemyKingdoms(otherKingdom));
            foreach (Kingdom commonEnemy in commonEnemies)
            {
                TextObject textObject = null;
                if (explanation != null)
                {
                    textObject = new TextObject("{=RqQ4oqvl}War with {ENEMY_KINGDOM}");
                    textObject.SetTextVariable("ENEMY_KINGDOM", commonEnemy.Name);
                }
                explainedNumber.Add(Scores.HasCommonEnemy, textObject);
            }

            IEnumerable<Kingdom> alliedEnemies = Kingdom.All.Except(new[] { kingdom, otherKingdom }).Where(curKingdom => FactionManager.IsAlliedWithFaction(otherKingdom, curKingdom) && FactionManager.IsAtWarAgainstFaction(kingdom, curKingdom));
            IEnumerable<Kingdom> alliedNeutrals = Kingdom.All.Except(new[] { kingdom, otherKingdom }).Where(curKingdom => FactionManager.IsAlliedWithFaction(otherKingdom, curKingdom) && !FactionManager.IsAtWarAgainstFaction(kingdom, curKingdom));
            foreach (Kingdom alliedEnemy in alliedEnemies)
            {
                TextObject textObject = null;
                if (explanation != null)
                {
                    textObject = new TextObject("{=cmOSpfyW}Alliance with {ALLIED_KINGDOM}");
                    textObject.SetTextVariable("ALLIED_KINGDOM", alliedEnemy.Name);
                }
                explainedNumber.Add(Scores.ExistingAllianceWithEnemy, textObject);
            }
            foreach (Kingdom alliedNeutral in alliedNeutrals)
            {
                TextObject textObject = null;
                if (explanation != null)
                {
                    textObject = new TextObject("{=cmOSpfyW}Alliance with {ALLIED_KINGDOM}");
                    textObject.SetTextVariable("ALLIED_KINGDOM", alliedNeutral.Name);
                }
                explainedNumber.Add(Scores.ExistingAllianceWithNeutral, textObject);
            }

            // relation modifier
            float relationModifier = MBMath.ClampFloat((float)Math.Log((kingdom.Leader.GetRelation(otherKingdom.Leader) + 100f) / 100f, 1.5), -1, 1);
            explainedNumber.Add(Scores.Relationship * relationModifier, _relationship);

            // expansionism modifier
            if (otherKingdom.GetExpansionismDiplomaticPenalty() < 0)
            {
                TextObject textObject = null;
                if (explanation != null)
                {
                    textObject = new TextObject("{=CxdpR6w4}Expansionism");
                }
                explainedNumber.Add(otherKingdom.GetExpansionismDiplomaticPenalty(), textObject);
            }
            return explainedNumber;
        }

        public virtual bool ShouldFormBidirectional(Kingdom kingdom, Kingdom otherKingdom)
        {
            return ShouldForm(kingdom, otherKingdom) && ShouldForm(otherKingdom, kingdom);
        }

        public virtual bool ShouldForm(Kingdom kingdom, Kingdom otherKingdom)
        {
            return GetScore(kingdom, otherKingdom).ResultNumber >= ScoreThreshold;
        }
    }

    public interface IScores
    {
        int Base { get; }
        int BelowMedianStrength { get; }
        int HasCommonEnemy { get; }
        int ExistingAllianceWithEnemy { get; }
        int ExistingAllianceWithNeutral { get; }
        int Relationship { get; }
    }
}
