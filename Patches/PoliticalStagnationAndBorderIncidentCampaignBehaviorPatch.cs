using HarmonyLib;
using Helpers;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

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

            return !PlayerHelpers.IsPlayerLeaderOfFaction(kingdom);
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
                if (CooldownManager.HasActiveWarCooldown(kingdom, faction2))
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
