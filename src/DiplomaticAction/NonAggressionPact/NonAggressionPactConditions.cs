using Diplomacy.DiplomaticAction.GenericConditions;
using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.NonAggressionPact
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
            new HasEnoughScoreCondition(),
            new NotRebelKingdomCondition()
        };

        public NonAggressionPactConditions() : base(_formNonAggressionPactConditions) { }
    }
}
