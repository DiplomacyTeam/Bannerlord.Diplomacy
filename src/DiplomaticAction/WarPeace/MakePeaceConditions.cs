using Diplomacy.DiplomaticAction.GenericConditions;
using Diplomacy.DiplomaticAction.WarPeace.Conditions;
using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.WarPeace
{
    class MakePeaceConditions : AbstractConditionEvaluator<MakePeaceConditions>
    {
        private static readonly List<IDiplomacyCondition> _peaceConditions = new List<IDiplomacyCondition>
        {
            new SatisfiesQuestConditionsForPeaceCondition(),
            new HasEnoughGoldForPeaceCondition(),
            new HasEnoughInfluenceForPeaceCondition(),
            new HasEnoughTimeElapsedForPeaceCondition(),
            new NoPlayerSiegeCondition(),
            new NotRebelKingdomCondition()
        };

        public MakePeaceConditions() : base(_peaceConditions) { }
    }
}
