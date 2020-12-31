using Diplomacy.DiplomaticAction.WarPeace.Conditions;
using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.WarPeace
{
    class MakePeaceConditions : AbstractConditionEvaluator<MakePeaceConditions>
    {
        private static List<IDiplomacyCondition> _peaceConditions = new List<IDiplomacyCondition>
        {
            new SatisfiesQuestConditionsForPeaceCondition(),
            new HasEnoughGoldForPeaceCondition(),
            new HasEnoughInfluenceForPeaceCondition(),
            new HasEnoughTimeElapsedForPeaceCondition(),
            new NoPlayerSiegeCondition()
        };

        public MakePeaceConditions() : base(_peaceConditions) { }
    }
}
