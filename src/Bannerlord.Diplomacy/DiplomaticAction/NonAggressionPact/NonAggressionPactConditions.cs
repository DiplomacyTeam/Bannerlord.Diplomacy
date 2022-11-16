using Diplomacy.DiplomaticAction.GenericConditions;

using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.NonAggressionPact
{
    internal class NonAggressionPactConditions : AbstractConditionEvaluator<NonAggressionPactConditions>
    {
        private static readonly List<IDiplomacyCondition> Conditions = new()
        {
            new AtPeaceCondition(),
            new NotInAllianceCondition(),
            new HasEnoughInfluenceCondition(),
            new HasEnoughGoldCondition(),
            new NoNonAggressionPactCondition(),
            new HasEnoughScoreCondition(),
            new NotRebelKingdomCondition(),
            new NotEliminatedCondition()
        };

        public NonAggressionPactConditions() : base(Conditions) { }
    }
}