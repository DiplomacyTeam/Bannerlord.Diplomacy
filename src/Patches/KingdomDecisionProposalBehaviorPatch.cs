using Diplomacy.DiplomaticAction.WarPeace;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace Diplomacy.Patches
{
    [HarmonyPatch(typeof(KingdomDecisionProposalBehavior))]
    class KingdomDecisionProposalBehaviorPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ConsiderWar")]
        public static bool ConsiderWarDecisionPatch(Clan clan, Kingdom kingdom, IFaction otherFaction, bool __result)
        {
            Kingdom otherKingdom = otherFaction as Kingdom;
            if (otherKingdom != null && !DeclareWarConditions.Instance.CanApply(kingdom, otherKingdom, bypassCosts: true))
            {
                __result = false;
                return false;
            }
            else
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
            if (otherKingdom != null && !MakePeaceConditions.Instance.CanApply(kingdom, otherKingdom, bypassCosts: true))
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
