using DiplomacyFixes.ViewModel;
using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomClan;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;

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
            __instance.Diplomacy = new KingdomDiplomacyVMExtensionVM(forceDecideDecisionAction);

            KingdomCategoryVM currentCategoryFieldInfo =
                (KingdomCategoryVM)typeof(KingdomManagementVM).GetField("_currentCategory", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            MethodInfo setCurrentCategoryMethodInfo = __instance.GetType().GetMethod("SetCurrentCategory", BindingFlags.NonPublic | BindingFlags.Instance);
            if (currentCategoryFieldInfo is KingdomClanVM)
            {
                setCurrentCategoryMethodInfo.Invoke(__instance, new object[] { __instance.Clan });
            }
            else if (currentCategoryFieldInfo is KingdomDiplomacyVM)
            {
                setCurrentCategoryMethodInfo.Invoke(__instance, new object[] { __instance.Diplomacy });
            }
        }
    }
}
