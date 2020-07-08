using DiplomacyFixes.DiplomaticAction.Alliance.Conditions;
using System.Collections.Generic;

namespace DiplomacyFixes.DiplomaticAction.Alliance
{
    class BreakAllianceConditions : AbstractConditionEvaluator<BreakAllianceConditions>
    {
        private static List<IDiplomacyCondition> _breakAllianceConditions = new List<IDiplomacyCondition>
        {
            new TimeElapsedSinceAllianceFormedCondition()
        };

        protected override List<IDiplomacyCondition> Conditions => _breakAllianceConditions;
    }
}
