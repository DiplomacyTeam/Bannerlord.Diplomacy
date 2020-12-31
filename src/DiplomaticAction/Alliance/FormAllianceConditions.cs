using Diplomacy.DiplomaticAction.Alliance.Conditions;
using Diplomacy.DiplomaticAction.GenericConditions;
using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.Alliance
{
    class FormAllianceConditions : AbstractConditionEvaluator<FormAllianceConditions>
    {
        private static List<IDiplomacyCondition> _formAllianceConditions = new List<IDiplomacyCondition>
        {
            new AlliancesEnabledCondition(),
            new AtPeaceCondition(),
            new TimeElapsedSinceLastWarCondition(),
            new NotInAllianceCondition(),
            new HasEnoughInfluenceCondition(),
            new HasEnoughGoldCondition(),
            new HasEnoughScoreCondition()
        };

        public FormAllianceConditions() : base(_formAllianceConditions) { }
    }
}
