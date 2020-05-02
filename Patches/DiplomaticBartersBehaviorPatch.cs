using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.BarterBehaviors;

namespace DiplomacyFixes.Patches
{
    [HarmonyPatch(typeof(DiplomaticBartersBehavior))]
    class DiplomaticBartersBehaviorPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ConsiderWar")]
        public static bool ConsiderWarPatch(IFaction mapFaction, IFaction otherMapFaction)
        {
            return !CooldownManager.HasDeclareWarCooldown(mapFaction, otherMapFaction);
        }
    }
}
