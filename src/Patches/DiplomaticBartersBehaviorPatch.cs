using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.BarterBehaviors;

namespace Diplomacy.Patches
{
    /// <summary>
    /// Blocks AI from declaring war due to the AI diplomatic barter behavior.
    /// The other way for the AI to consider war is via a kingdom decision proposal.
    /// </summary>
    [HarmonyPatch(typeof(DiplomaticBartersBehavior))]
    class DiplomaticBartersBehaviorPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ConsiderWar")]
        public static bool ConsiderWarPatch(Clan clan, IFaction otherMapFaction)
            => !CooldownManager.HasDeclareWarCooldown(clan, otherMapFaction, out _);
    }
}
