using HarmonyLib;
using Helpers;
using System;
using System.Collections.Generic;
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

            List<IFaction> possibleKingdomsToDeclareWar = FactionHelper.GetPossibleKingdomsToDeclareWar(kingdom);
            float num = 0f;
            IFaction faction = null;
            foreach (IFaction faction2 in possibleKingdomsToDeclareWar)
            {
                if (!WarAndPeaceConditions.CanDeclareWar(kingdom, faction2))
                {
                    continue;
                }

                IEnumerable<LogEntry> gameActionLogs = Campaign.Current.LogEntryHistory.GameActionLogs;
                bool flag = false;
                foreach (LogEntry logEntry in gameActionLogs)
                {
                    if (logEntry is MakePeaceLogEntry && CampaignTime.Now.ToHours - logEntry.GameTime.ToHours < 600.0 && ((((MakePeaceLogEntry)logEntry).Faction1 == kingdom.MapFaction && ((MakePeaceLogEntry)logEntry).Faction2 == faction2.MapFaction) || (((MakePeaceLogEntry)logEntry).Faction1 == faction2.MapFaction && ((MakePeaceLogEntry)logEntry).Faction2 == kingdom.MapFaction)))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    float scoreOfDeclaringWar = Campaign.Current.Models.DiplomacyModel.GetScoreOfDeclaringWar(kingdom, faction2, false);
                    if (scoreOfDeclaringWar > num)
                    {
                        faction = faction2;
                        num = scoreOfDeclaringWar;
                    }
                }
            }
            if (faction != null && MBRandom.RandomFloat < Math.Min(0.5f, num / 400000f))
            {
                DeclareWarAction.ApplyDeclareWarOverProvocation(kingdom, faction);
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

            List<IFaction> possibleKingdomsToDeclarePeace = FactionHelper.GetPossibleKingdomsToDeclarePeace(kingdom);
            float num = 0f;
            IFaction faction = null;
            int num2 = 0;
            foreach (IFaction faction2 in possibleKingdomsToDeclarePeace)
            {
                if (!WarAndPeaceConditions.CanProposePeace(kingdom, faction2))
                {
                    continue;
                }

                float scoreOfDeclaringPeace = Campaign.Current.Models.DiplomacyModel.GetScoreOfDeclaringPeace(kingdom, faction2);
                float scoreOfDeclaringPeace2 = Campaign.Current.Models.DiplomacyModel.GetScoreOfDeclaringPeace(faction2, kingdom);
                if (Math.Max(scoreOfDeclaringPeace, scoreOfDeclaringPeace2) > num && scoreOfDeclaringPeace > 0f)
                {
                    faction = faction2;
                    if (scoreOfDeclaringPeace2 < 0f)
                    {
                        num2 = -(int)(scoreOfDeclaringPeace2 + 1f);
                    }
                    num = Math.Max(scoreOfDeclaringPeace, scoreOfDeclaringPeace2);
                }
            }
            float num3 = (kingdom.Leader.Gold < 10000) ? 3f : ((float)(3.0 + (Math.Sqrt((double)((float)kingdom.Leader.Gold / 10000f)) - 1.0)));
            int num4 = (int)Math.Min((float)kingdom.Leader.Gold / num3, num / 2f);
            if (num > 0f && num2 < num4)
            {
                KingdomPeaceAction.ApplyPeace(kingdom, faction, num2);
            }

            return false;
        }
    }
}
