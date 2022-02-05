using Diplomacy.DiplomaticAction.Conditioning;
using Diplomacy.DiplomaticAction.Conditioning.GenericConditions;
using Diplomacy.DiplomaticAction.NonAggressionPact;
using Diplomacy.DiplomaticAction.WarPeace.Conditions;

using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.WarPeace
{
    internal sealed class DeclareWarConditions : AbstractConditionEvaluator<DeclareWarConditions>
    {
        private static readonly List<AbstractDiplomacyCondition> Conditions = new()
        {
            new HasEnoughInfluenceForWarCondition(),
            new NoNonAggressionPactCondition(),
            new NotInAllianceCondition(),
            new AtPeaceCondition(),
            new NotRebelKingdomCondition()
        };

        public DeclareWarConditions() : base(Conditions) { }
    }
}