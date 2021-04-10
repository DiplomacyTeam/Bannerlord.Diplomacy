using Diplomacy.DiplomaticAction.GenericConditions;
using Diplomacy.DiplomaticAction.NonAggressionPact;
using Diplomacy.DiplomaticAction.WarPeace.Conditions;

using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.WarPeace
{
    internal sealed class DeclareWarConditions : AbstractConditionEvaluator<DeclareWarConditions>
    {
        private static readonly List<IDiplomacyCondition> _warConditions = new List<IDiplomacyCondition>
        {
            new HasEnoughInfluenceForWarCondition(),
            new HasLowWarExhaustionCondition(),
            new NoNonAggressionPactCondition(),
            new NotInAllianceCondition(),
            new AtPeaceCondition(),
            new NotRebelKingdomCondition()
        };

        public DeclareWarConditions() : base(_warConditions) { }
    }
}
