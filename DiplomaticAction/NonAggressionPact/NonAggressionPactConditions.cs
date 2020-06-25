using DiplomacyFixes.DiplomaticAction.GenericConditions;
using System.Collections.Generic;

namespace DiplomacyFixes.DiplomaticAction.NonAggressionPact
{
    class NonAggressionPactConditions : DiplomaticConditions
    {
        private static List<IDiplomacyCondition> _formNonAggressionPactConditions = new List<IDiplomacyCondition>
        {
            new AtPeaceCondition(),
            new NotAlreadyInAllianceCondition(),
            new HasEnoughInfluenceCondition(),
            new HasEnoughGoldCondition(),
            new NoNonAggressionPactCondition()
        };

        public NonAggressionPactConditions() : base(_formNonAggressionPactConditions) { }

        public static DiplomaticConditions Instance { 
            get {
                if(_instance == null)
                {
                    _instance = new NonAggressionPactConditions();
                }
                return _instance;
            } }
    }
}
