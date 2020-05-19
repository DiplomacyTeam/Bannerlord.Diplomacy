using DiplomacyFixes.ViewModel;
using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomClan;

namespace DiplomacyFixes.Patches
{
    [HarmonyPatch(typeof(KingdomManagementVM))]
    class KingdomManagementVMPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("RefreshValues")]
        public static void RefreshValuesPatch(KingdomManagementVM __instance)
        {
            MethodInfo forceDecideDecision = __instance.GetType().GetMethod("ForceDecideDecision", BindingFlags.NonPublic | BindingFlags.Instance);
            Action<KingdomDecision> forceDecideDecisionAction = (Action<KingdomDecision>)Delegate.CreateDelegate(typeof(Action<KingdomDecision>), __instance, forceDecideDecision);
            __instance.Clan = new KingdomClanVMExtensionVM(forceDecideDecisionAction);

            KingdomCategoryVM currentCategoryFieldInfo =
                (KingdomCategoryVM)typeof(KingdomManagementVM).GetField("_currentCategory", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            if (currentCategoryFieldInfo is KingdomClanVM)
            {
                MethodInfo setCurrentCategoryMethodInfo = __instance.GetType().GetMethod("SetCurrentCategory", BindingFlags.NonPublic | BindingFlags.Instance);
                setCurrentCategoryMethodInfo.Invoke(__instance, new object[] { __instance.Clan });
            }
        }
    }
}
