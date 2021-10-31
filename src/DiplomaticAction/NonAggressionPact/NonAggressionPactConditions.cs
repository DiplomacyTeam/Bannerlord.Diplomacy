using Diplomacy.DiplomaticAction.GenericConditions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

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

        private static readonly List<IDiplomacyCondition> ConditionsWithoutScore =
            Conditions.Where(x => x.GetType() != typeof(HasEnoughScoreCondition)).ToList();

        public NonAggressionPactConditions() : base(Conditions) { }
    }
}
