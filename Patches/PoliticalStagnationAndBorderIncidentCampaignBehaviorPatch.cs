using HarmonyLib;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;

namespace DiplomacyFixes.Patches
{
    [HarmonyPatch(typeof(PoliticalStagnationAndBorderIncidentCampaignBehavior))]
    class PoliticalStagnationAndBorderIncidentCampaignBehaviorPatch
    {

        [HarmonyPrefix]
        [HarmonyPatch("ThinkAboutDeclaringWar")]
        public static bool ThinkAboutDeclaringWarPatch(Kingdom kingdom)
        {
            if (!Settings.Instance.PlayerDiplomacyControl)
            {
                return true;
            }

            if (PlayerHelpers.IsPlayerLeaderOfFaction(kingdom))
            {
                return false;
            }

            IEnumerable<Kingdom> possibleKingdomsToDeclareWar = FactionHelper.GetPossibleKingdomsToDeclareWar(kingdom).Select(faction => faction as Kingdom);
            float num = 0f;
            Kingdom otherKingdom = null;
            foreach (Kingdom currentKingdom in possibleKingdomsToDeclareWar)
            {
                if (!WarAndPeaceConditions.CanDeclareWar(kingdom, currentKingdom))
                {
                    continue;
                }

                IEnumerable<LogEntry> gameActionLogs = Campaign.Current.LogEntryHistory.GameActionLogs;
                bool flag = false;
                foreach (LogEntry logEntry in gameActionLogs)
                {
                    if (logEntry is MakePeaceLogEntry && CampaignTime.Now.ToHours - logEntry.GameTime.ToHours < 600.0 && ((((MakePeaceLogEntry)logEntry).Faction1 == kingdom.MapFaction && ((MakePeaceLogEntry)logEntry).Faction2 == currentKingdom.MapFaction) || (((MakePeaceLogEntry)logEntry).Faction1 == currentKingdom.MapFaction && ((MakePeaceLogEntry)logEntry).Faction2 == kingdom.MapFaction)))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    float scoreOfDeclaringWar = Campaign.Current.Models.DiplomacyModel.GetScoreOfDeclaringWar(kingdom, currentKingdom, false);
                    if (scoreOfDeclaringWar > num)
                    {
                        otherKingdom = currentKingdom;
                        num = scoreOfDeclaringWar;
                    }
                }
            }
            if (otherKingdom != null && MBRandom.RandomFloat < Math.Min(0.5f, num / 400000f))
            {
                DeclareWarAction.ApplyDeclareWarOverProvocation(kingdom, otherKingdom);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("ThinkAboutDeclaringPeace")]
        public static bool ThinkAboutDeclaringPeacePatch(Kingdom kingdom)
        {
            if (!Settings.Instance.PlayerDiplomacyControl)
            {
                return true;
            }

            if (PlayerHelpers.IsPlayerLeaderOfFaction(kingdom))
            {
                return false;
            }

            IEnumerable<Kingdom> possibleKingdomsToDeclarePeace = FactionHelper.GetPossibleKingdomsToDeclarePeace(kingdom).Select(faction => faction as Kingdom);
            float maximumScoreOfDeclaringPiece = 0f;
            Kingdom otherKingdom = null;
            foreach (Kingdom currentKingdom in possibleKingdomsToDeclarePeace)
            {
                if (!WarAndPeaceConditions.CanProposePeace(kingdom, currentKingdom))
                {
                    continue;
                }

                float scoreOfDeclaringPeace = Campaign.Current.Models.DiplomacyModel.GetScoreOfDeclaringPeace(kingdom, currentKingdom);
                float scoreOfDeclaringPeace2 = Campaign.Current.Models.DiplomacyModel.GetScoreOfDeclaringPeace(currentKingdom, kingdom);
                if (Math.Max(scoreOfDeclaringPeace, scoreOfDeclaringPeace2) > maximumScoreOfDeclaringPiece && scoreOfDeclaringPeace > 0f)
                {
                    otherKingdom = currentKingdom;
                    maximumScoreOfDeclaringPiece = Math.Max(scoreOfDeclaringPeace, scoreOfDeclaringPeace2);
                }
            }

            if (maximumScoreOfDeclaringPiece > 0f)
            {
                KingdomPeaceAction.ApplyPeace(kingdom, otherKingdom);
            }

            return false;
        }
    }
}
