using Diplomacy.PatchTools;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement;

namespace Diplomacy.Patches
{
    internal sealed class KingdomManagementVMPatch : PatchClass<KingdomManagementVMPatch>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            return new Patch[]
            {
                new Postfix(nameof(FinalizeFix), typeof(KingdomManagementVM), "OnFinalize"),
            };
        }

        private static void FinalizeFix(KingdomManagementVM __instance)
        {
            __instance.Diplomacy.OnFinalize();
        }
    }
}