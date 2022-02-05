using System;

namespace Diplomacy.DiplomaticAction.Conditioning
{
    [Flags]
    internal enum DiplomacyConditionType : byte
    {
        None = 0,
        ActionSpecific = 1,
        CostRelated = 2,
        ScoreRelated = 4,
        TimeRelated = 8,
        //Popular cases:
        OnlyBasics = ActionSpecific,
        BypassCosts = ActionSpecific | ScoreRelated | TimeRelated,
        BypassScores = ActionSpecific | CostRelated | TimeRelated,
        BypassCooldowns = ActionSpecific | CostRelated | ScoreRelated,
        All = ActionSpecific | CostRelated | ScoreRelated | TimeRelated
    }
}
