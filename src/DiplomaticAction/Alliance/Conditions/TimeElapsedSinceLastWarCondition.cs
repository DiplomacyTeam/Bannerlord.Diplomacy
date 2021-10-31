using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Alliance.Conditions
{
    internal sealed class TimeElapsedSinceLastWarCondition : AbstractTimeCondition
    {
        private static readonly TextObject _TTooSoon = new("{=DrnXprup}You have been at war too recently to consider an alliance. It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.");
        private const int MinimumTimeFromLastWar = 30;

        protected override TextObject GetFailedConditionText() => _TTooSoon.CopyTextObject();

        protected override bool HasEnoughTimeElapsed(Kingdom kingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType, out float elapsedDaysUntilNow, out int requiredDays)
        {
            requiredDays = MinimumTimeFromLastWar;
            IEnumerable<LogEntry> gameActionLogs = Campaign.Current.LogEntryHistory.GameActionLogs;

            var lastPeaceTime = CampaignTime.Never;
            foreach (var logEntry in gameActionLogs)
            {
                if (logEntry is MakePeaceLogEntry entry
                    && ((entry.Faction1 == kingdom.MapFaction && entry.Faction2 == otherKingdom.MapFaction) || (entry.Faction1 == otherKingdom.MapFaction && entry.Faction2 == kingdom.MapFaction)))
                {
                    lastPeaceTime = entry.GameTime;
                    break;
                }
            }

            if (lastPeaceTime == CampaignTime.Never)
            {
                elapsedDaysUntilNow = default;
                return true;
            }
            else
            {
                elapsedDaysUntilNow = lastPeaceTime.ElapsedDaysUntilNow;
                return elapsedDaysUntilNow >= MinimumTimeFromLastWar;
            }
        }
    }
}
