using DiplomacyFixes.DiplomaticAction.Alliance.Conditions;
using DiplomacyFixes.DiplomaticAction.GenericConditions;
using System.Collections.Generic;

namespace DiplomacyFixes.DiplomaticAction.Alliance
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

        protected override List<IDiplomacyCondition> Conditions => _formAllianceConditions;
    }
}
