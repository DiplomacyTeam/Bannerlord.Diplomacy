using Diplomacy.DiplomaticAction.Alliance.Conditions;
using Diplomacy.DiplomaticAction.GenericConditions;

using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.Alliance
{
    class FormAllianceConditions : AbstractConditionEvaluator<FormAllianceConditions>
    {
        private static readonly List<IDiplomacyCondition> Conditions = new()
        {
            new AlliancesEnabledCondition(),
            new AtPeaceCondition(),
            new TimeElapsedSinceLastWarCondition(),
            new NotInAllianceCondition(),
            new HasEnoughInfluenceCondition(),
            new HasEnoughGoldCondition(),
            new HasEnoughScoreCondition(),
            new NotRebelKingdomCondition(),
            new NotEliminatedCondition()
        };

        public FormAllianceConditions() : base(Conditions) { }
    }
}