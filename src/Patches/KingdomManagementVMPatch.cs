using Diplomacy.ViewModel;
using Diplomacy.PatchTools;

using HarmonyLib;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.ViewModelCollection;

namespace Diplomacy.Patches
{
    internal sealed class KingdomManagementVMPatch : PatchClass<KingdomManagementVMPatch, KingdomManagementVM>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Prefix(nameof(ExecuteClosePrefix), "ExecuteClose"),
            new Postfix(nameof(RefreshValuesPostfix), nameof(KingdomManagementVM.RefreshValues)),
        };

        private static void ExecuteClosePrefix(KingdomManagementVM __instance)
        {
            ((KingdomDiplomacyVMExtensionVM)__instance.Diplomacy).OnClose();
            ((KingdomClanVMExtensionVM)__instance.Clan).OnClose();
        }

        private static readonly Reflect.Method<KingdomManagementVM> _ForceDecideDecisionRM = new("ForceDecideDecision");
        private static readonly Reflect.Method<KingdomManagementVM> _SetSelectedCategoryRM = new("SetSelectedCategory");

        private delegate void SetSelectedCategoryDel(KingdomManagementVM instance, int index);

        private static readonly SetSelectedCategoryDel _SetSelectedCategory = _SetSelectedCategoryRM.GetOpenDelegate<SetSelectedCategoryDel>();

        private static void RefreshValuesPostfix(KingdomManagementVM __instance)
        {
            var forceDecideDecision = _ForceDecideDecisionRM.GetDelegate<Action<KingdomDecision>>(__instance);

            __instance.Clan = new KingdomClanVMExtensionVM(forceDecideDecision);
            __instance.Diplomacy = new KingdomDiplomacyVMExtensionVM(forceDecideDecision);

            var currentCategory = (int)AccessTools.Field(typeof(KingdomManagementVM), "_currentCategory").GetValue(__instance);

            if (currentCategory == 0 || currentCategory == 4) // Clan || Diplomacy
                _SetSelectedCategory(__instance, currentCategory);

            // FIXME: The current SetSelectedCategory only has one version, and it takes an int. The code that was here passed
            // it a VM, as can be seen below. I corrected this so that it made sense, but I don't see how this wouldn't have caused
            // massive problems already, so we must test whether this broke anything. I suspect something went wrong here in
            // compatibility patching prior to my tenure.

            /*
             *          var setCurrentCategoryMethodInfo = __instance.GetType()
             *              .GetMethod("SetSelectedCategory", BindingFlags.NonPublic | BindingFlags.Instance);
             *
             * // ...
             *
             *          if (currentCategory == 0)
             *              setCurrentCategoryMethodInfo.Invoke(__instance, new object[] { __instance.Clan });
             *          else if (currentCategory == 4)
             *              setCurrentCategoryMethodInfo.Invoke(__instance, new object[] { __instance.Diplomacy });
             */
        }
    }
}
