using DiplomacyFixes.DiplomaticAction.GenericConditions;
using System.Collections.Generic;

namespace DiplomacyFixes.DiplomaticAction.NonAggressionPact
{
    class NonAggressionPactConditions : AbstractConditionEvaluator<NonAggressionPactConditions>
    {
        private static List<IDiplomacyCondition> _formNonAggressionPactConditions = new List<IDiplomacyCondition>
        {
            new AtPeaceCondition(),
            new NotInAllianceCondition(),
            new HasEnoughInfluenceCondition(),
            new HasEnoughGoldCondition(),
            new NoNonAggressionPactCondition(),
            new HasEnoughScoreCondition()
        };

        public NonAggressionPactConditions() : base(_formNonAggressionPactConditions) { }
    }
}
