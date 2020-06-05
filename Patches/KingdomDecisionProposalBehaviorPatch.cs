using DiplomacyFixes.WarPeace;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace DiplomacyFixes.Patches
{
    [HarmonyPatch(typeof(KingdomDecisionProposalBehavior))]
    class KingdomDecisionProposalBehaviorPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ConsiderWar")]
        public static bool ConsiderWarDecisionPatch(Clan clan, Kingdom kingdom, IFaction otherFaction, bool __result)
        {
            Kingdom otherKingdom = otherFaction as Kingdom;
            if (otherKingdom != null && !WarAndPeaceConditions.CanDeclareWar(kingdom, otherKingdom))
            {
                __result = false;
                return false;
            } else
            {
                return true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("ConsiderPeace")]
        public static bool ConsiderPeacePatch(Clan clan, Clan otherClan, Kingdom kingdom, IFaction otherFaction, out MakePeaceKingdomDecision decision, bool __result)
        {
            decision = null;
            Kingdom otherKingdom = otherFaction as Kingdom;
            if (otherKingdom != null && !WarAndPeaceConditions.CanProposePeace(kingdom, otherKingdom))
            {
                __result = false;
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
