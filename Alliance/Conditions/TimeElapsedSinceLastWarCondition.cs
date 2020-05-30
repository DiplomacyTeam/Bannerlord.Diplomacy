using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.Localization;

namespace DiplomacyFixes.Alliance.Conditions
{
    internal class TimeElapsedSinceLastWarCondition : IDiplomacyCondition
    {
        private const string TOO_SOON = "{=DrnXprup}You have been at war too recently to consider an alliance. It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.";
        private const double MinimumTimeFromLastWar = 30.0;

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            IEnumerable<LogEntry> gameActionLogs = Campaign.Current.LogEntryHistory.GameActionLogs;

            CampaignTime lastPeaceTime = CampaignTime.Never;
            foreach (LogEntry logEntry in gameActionLogs)
            {
                if (logEntry is MakePeaceLogEntry
                    && ((((MakePeaceLogEntry)logEntry).Faction1 == kingdom.MapFaction && ((MakePeaceLogEntry)logEntry).Faction2 == otherKingdom.MapFaction) || (((MakePeaceLogEntry)logEntry).Faction1 == otherKingdom.MapFaction && ((MakePeaceLogEntry)logEntry).Faction2 == kingdom.MapFaction)))
                {
                    lastPeaceTime = logEntry.GameTime;
                    break;
                }
            }

            double daysSinceLastWar = CampaignTime.Now.ToDays - lastPeaceTime.ToDays;
            bool hasEnoughTimeElapsed = lastPeaceTime == CampaignTime.Never || daysSinceLastWar > MinimumTimeFromLastWar;
            if (!hasEnoughTimeElapsed)
            {
                textObject = new TextObject(TOO_SOON);
                textObject.SetTextVariable("ELAPSED_DAYS", (int)daysSinceLastWar);
                textObject.SetTextVariable("REQUIRED_DAYS", (int)MinimumTimeFromLastWar);
            }
            return hasEnoughTimeElapsed;
        }
    }
}