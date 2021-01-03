using Diplomacy.ViewModel;

using HarmonyLib;

using System;
using System.Reflection;

using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomClan;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;

namespace Diplomacy.Patches
{
    [HarmonyPatch(typeof(KingdomManagementVM))]
    internal static class KingdomManagementVMPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("RefreshValues")]
        static void RefreshValuesPatch(KingdomManagementVM __instance)
        {
            // FIXME: Upgrade all this uncached reflection to use cached delegates
            var forceDecideDecision = __instance.GetType().GetMethod("ForceDecideDecision", BindingFlags.NonPublic | BindingFlags.Instance);
            var forceDecideDecisionAction = (Action<KingdomDecision>)Delegate.CreateDelegate(typeof(Action<KingdomDecision>), __instance, forceDecideDecision);
            __instance.Clan = new KingdomClanVMExtensionVM(forceDecideDecisionAction);
            __instance.Diplomacy = new KingdomDiplomacyVMExtensionVM(forceDecideDecisionAction);

            int currentCategoryFieldInfo = (int)typeof(KingdomManagementVM)
                .GetField("_currentCategory", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(__instance);

            var setCurrentCategoryMethodInfo = __instance.GetType()
                .GetMethod("SetSelectedCategory", BindingFlags.NonPublic | BindingFlags.Instance);

            // FIXME: Import whatever private enum these numbers stand for
            if (currentCategoryFieldInfo == 0)
                setCurrentCategoryMethodInfo.Invoke(__instance, new object[] { __instance.Clan });
            else if (currentCategoryFieldInfo == 4)
                setCurrentCategoryMethodInfo.Invoke(__instance, new object[] { __instance.Diplomacy });
        }

        [HarmonyPrefix]
        [HarmonyPatch("ExecuteClose")]
        static void ExecuteClosePatch(KingdomManagementVM __instance)
        {
            ((KingdomDiplomacyVMExtensionVM)__instance.Diplomacy).OnClose();
            ((KingdomClanVMExtensionVM)__instance.Clan).OnClose();
        }
    }
}
