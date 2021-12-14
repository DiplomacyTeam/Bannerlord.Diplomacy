using Diplomacy.DiplomaticAction.Alliance.Conditions;

using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.Alliance
{
    class BreakAllianceConditions : AbstractConditionEvaluator<BreakAllianceConditions>
    {
        private static readonly List<IDiplomacyCondition> Conditions = new()
        {
            new TimeElapsedSinceAllianceFormedCondition()
        };
        public BreakAllianceConditions() : base(Conditions) { }
    }
}