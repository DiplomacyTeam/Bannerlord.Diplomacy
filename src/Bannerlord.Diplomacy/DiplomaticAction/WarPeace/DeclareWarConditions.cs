using Diplomacy.DiplomaticAction.GenericConditions;
using Diplomacy.DiplomaticAction.NonAggressionPact;
using Diplomacy.DiplomaticAction.WarPeace.Conditions;

using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.WarPeace
{
    internal sealed class DeclareWarConditions : AbstractConditionEvaluator<DeclareWarConditions>
    {
        private static readonly List<IDiplomacyCondition> Conditions = new()
        {
            new HasEnoughInfluenceForWarCondition(),
            new NoNonAggressionPactCondition(),
            new NotInAllianceCondition(),
            new AtPeaceCondition(),
            new HasEnoughTimeElapsedForWarCondition(),
            new NotRebelKingdomCondition(),
            new NotEliminatedCondition()
        };

        public DeclareWarConditions() : base(Conditions) { }
    }
}