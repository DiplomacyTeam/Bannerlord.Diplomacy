using TaleWorlds.Library;
using Diplomacy.PatchTools;

using System.Collections.Generic;
using System.Reflection;
using System;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;

namespace Diplomacy.Patches
{
    internal class ViewModelPatch : PatchClass<ViewModelPatch>
    {        protected override IEnumerable<Patch> Prepare()
        {
            return new Patch[]
            {
                new Prefix(nameof(FixInvoke), new Reflect.Method(typeof(Common), "InvokeWithLog", new Type[]{typeof(MethodInfo), typeof(object), typeof(object[])})),
            };
        }

        private static void FixInvoke(MethodInfo methodInfo, object obj, object[] args)
        {
            var instance = methodInfo.GetType().GetField("_instance", BindingFlags.NonPublic | BindingFlags.Instance);
            if (instance != null && instance.GetValue(methodInfo) is ViewModelMixin.KingdomTruceItemVmMixin)
            {
                var MixinInstance = (ViewModelMixin.KingdomTruceItemVmMixin) instance.GetValue(methodInfo);
                var BadVm = MixinInstance.GetType().BaseType.GetField("_vm", BindingFlags.Instance | BindingFlags.NonPublic);
                // Replace VM with obj
                BadVm.SetValue(MixinInstance, new WeakReference<KingdomTruceItemVM>((KingdomTruceItemVM)obj));

                // Replace private _factionN reference
                var vm = MixinInstance.GetType().GetField("_faction2", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
                vm.SetValue(MixinInstance, ((KingdomTruceItemVM) obj).Faction2);
            }
        }
    }
}
