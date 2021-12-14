using Diplomacy.DiplomaticAction.Alliance.Conditions;
using Diplomacy.DiplomaticAction.GenericConditions;

using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.Alliance
{
    internal sealed class FormAllianceConditions : AbstractConditionEvaluator<FormAllianceConditions>
    {
        private static readonly List<AbstractDiplomacyCondition> Conditions = new()
        {
            new AlliancesEnabledCondition(),
            new NotRebelKingdomCondition(),
            new AtPeaceCondition(),
            new TimeElapsedSinceLastWarCondition(),
            new NotInAllianceCondition(),
            new HasNoConflictingTermsCondition(),
            new HasEnoughInfluenceCondition(),
            new HasEnoughGoldCondition(),
            new HasEnoughScoreCondition()
        };

        public FormAllianceConditions() : base(Conditions) { }
    }
}