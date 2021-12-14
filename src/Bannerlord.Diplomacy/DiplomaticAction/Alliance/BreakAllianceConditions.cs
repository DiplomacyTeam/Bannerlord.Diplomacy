using Diplomacy.DiplomaticAction.Alliance.Conditions;

using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.Alliance
{
    internal sealed class BreakAllianceConditions : AbstractConditionEvaluator<BreakAllianceConditions>
    {
        private static readonly List<AbstractDiplomacyCondition> Conditions = new()
        {
            new TimeElapsedSinceAllianceFormedCondition(),
            new HasEnoughInfluenceCondition()
        };
        public BreakAllianceConditions() : base(Conditions) { }
    }
}