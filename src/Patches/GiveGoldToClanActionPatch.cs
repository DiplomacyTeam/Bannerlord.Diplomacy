using HarmonyLib;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Diplomacy.Patches
{
    [HarmonyPatch(typeof(GiveGoldToClanAction))]
    class GiveGoldToClanActionPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ApplyFromHeroToClan")]
        public static bool ApplyFromHeroToClanPatch(Hero giverHero, Clan clan, int amount)
        {
            typeof(GiveGoldToClanAction).GetMethod("ApplyInternal", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { giverHero, clan, amount });
            return false;
        }
    }
}
