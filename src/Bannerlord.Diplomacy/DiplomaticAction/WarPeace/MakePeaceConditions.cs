using Diplomacy.DiplomaticAction.Conditioning;
using Diplomacy.DiplomaticAction.Conditioning.GenericConditions;
using Diplomacy.DiplomaticAction.WarPeace.Conditions;

using System.Collections.Generic;

namespace Diplomacy.DiplomaticAction.WarPeace
{
    class MakePeaceConditions : AbstractConditionEvaluator<MakePeaceConditions>
    {
        private static readonly List<AbstractDiplomacyCondition> Conditions = new()
        {
            new SatisfiesQuestConditionsForPeaceCondition(),
            new HasEnoughGoldForPeaceCondition(),
            new HasEnoughInfluenceForPeaceCondition(),
            new HasEnoughTimeElapsedForPeaceCondition(),
            new NoPlayerSiegeCondition(),
            new NotRebelKingdomCondition()
        };

        public MakePeaceConditions() : base(Conditions) { }
    }
}