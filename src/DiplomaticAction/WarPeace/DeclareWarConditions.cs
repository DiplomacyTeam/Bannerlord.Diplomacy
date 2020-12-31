using Diplomacy.DiplomaticAction.GenericConditions;
using Diplomacy.DiplomaticAction.NonAggressionPact;
using Diplomacy.DiplomaticAction.WarPeace.Conditions;
using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.WarPeace
{
    class DeclareWarConditions : AbstractConditionEvaluator<DeclareWarConditions>
    {

        private static List<IDiplomacyCondition> _warConditions = new List<IDiplomacyCondition>
        {
            new HasEnoughInfluenceForWarCondition(),
            new HasLowWarExhaustionCondition(),
            new NoNonAggressionPactCondition(),
            new NotInAllianceCondition(),
            new AtPeaceCondition()
        };

        public DeclareWarConditions() : base(_warConditions) { }
    }
}
