using Diplomacy.DiplomaticAction.Alliance.Conditions;
using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.Alliance
{
    class BreakAllianceConditions : AbstractConditionEvaluator<BreakAllianceConditions>
    {
        private static readonly List<IDiplomacyCondition> _breakAllianceConditions = new List<IDiplomacyCondition>
        {
            new TimeElapsedSinceAllianceFormedCondition()
        };
        public BreakAllianceConditions() : base(_breakAllianceConditions) { }
    }
}
