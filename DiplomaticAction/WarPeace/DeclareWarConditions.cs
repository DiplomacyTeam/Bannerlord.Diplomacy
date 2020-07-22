using DiplomacyFixes.DiplomaticAction.GenericConditions;
using DiplomacyFixes.DiplomaticAction.NonAggressionPact;
using DiplomacyFixes.DiplomaticAction.WarPeace.Conditions;
using System.Collections.Generic;

namespace DiplomacyFixes.DiplomaticAction.WarPeace
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
