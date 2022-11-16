using Diplomacy.PatchTools;

using System;
using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;
using TaleWorlds.Library;

namespace Diplomacy.Patches
{
    internal class ViewModelPatch : PatchClass<ViewModelPatch>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            return new Patch[]
            {
                new Prefix(nameof(FixInvoke), new Reflect.Method(typeof(Common), "InvokeWithLog", new Type[]{typeof(MethodInfo), typeof(object), typeof(object[])})),
            };
        }

        // TODO: this is a terrible hack, hopefully I will get assitance with https://github.com/BUTR/Bannerlord.UIExtenderEx/discussions/159
        // and fix it properly
        private static void FixInvoke(MethodInfo methodInfo, object obj, object[] args)
        {
            var instance = methodInfo.GetType().GetField("_instance", BindingFlags.NonPublic | BindingFlags.Instance);
            if (instance != null && instance.GetValue(methodInfo) is ViewModelMixin.KingdomTruceItemVmMixin)
            {
                if (!Environment.StackTrace.Contains("OnRefresh"))
                {
                    var MixinInstance = (ViewModelMixin.KingdomTruceItemVmMixin) instance.GetValue(methodInfo);
                    var BadVm = MixinInstance.GetType().BaseType.GetField("_vm", BindingFlags.Instance | BindingFlags.NonPublic);
                    // Replace VM with obj
                    BadVm.SetValue(MixinInstance, new WeakReference<KingdomTruceItemVM>((KingdomTruceItemVM) obj));

                    // Replace private _factionn reference
                    var vm = MixinInstance.GetType().GetField("_faction2", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
                    vm.SetValue(MixinInstance, ((KingdomTruceItemVM) obj).Faction2);
                    MixinInstance.GetType().GetProperty("DiplomacyProperties").SetValue(MixinInstance, new ViewModel.DiplomacyPropertiesVM(((KingdomTruceItemVM) obj).Faction1, ((KingdomTruceItemVM) obj).Faction2));
                    MixinInstance.OnRefresh();
                }
            }
            else if (instance != null && instance.GetValue(methodInfo) is ViewModelMixin.KingdomWarItemVMMixin)
            {
                if (!Environment.StackTrace.Contains("OnRefresh"))
                {
                    var MixinInstance = (ViewModelMixin.KingdomWarItemVMMixin) instance.GetValue(methodInfo);
                    var BadVm = MixinInstance.GetType().BaseType.GetField("_vm", BindingFlags.Instance | BindingFlags.NonPublic);
                    // Replace VM with obj
                    BadVm.SetValue(MixinInstance, new WeakReference<KingdomWarItemVM>((KingdomWarItemVM) obj));

                    // Replace private _factionn reference
                    var vm = MixinInstance.GetType().GetField("_faction2", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
                    vm.SetValue(MixinInstance, ((KingdomWarItemVM) obj).Faction2);
                    MixinInstance.GetType().GetProperty("DiplomacyProperties").SetValue(MixinInstance, new ViewModel.DiplomacyPropertiesVM(((KingdomWarItemVM) obj).Faction1, ((KingdomWarItemVM) obj).Faction2));
                    MixinInstance.OnRefresh();
                }
            }
        }
    }
}