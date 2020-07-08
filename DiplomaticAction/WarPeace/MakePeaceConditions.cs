using DiplomacyFixes.DiplomaticAction.WarPeace.Conditions;
using System.Collections.Generic;

namespace DiplomacyFixes.DiplomaticAction.WarPeace
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
        protected override List<IDiplomacyCondition> Conditions => _peaceConditions;
    }
}
