using Diplomacy.DiplomaticAction.Conditioning;
using Diplomacy.DiplomaticAction.Conditioning.GenericConditions;

using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.NonAggressionPact
{
    internal class NonAggressionPactConditions : AbstractConditionEvaluator<NonAggressionPactConditions>
    {
        private static readonly List<AbstractDiplomacyCondition> Conditions = new()
        {
            new AtPeaceCondition(),
            new NotInAllianceCondition(),
            new HasEnoughInfluenceCondition(),
            new HasEnoughGoldCondition(),
            new NoNonAggressionPactCondition(),
            new HasEnoughScoreCondition(),
            new NotRebelKingdomCondition()
        };

        public NonAggressionPactConditions() : base(Conditions) { }
    }
}