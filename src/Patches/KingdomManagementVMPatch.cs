using System;
using System.Collections.Generic;
using System.Linq;
using Diplomacy.CivilWar.Factions;
using Diplomacy.Extensions;
using Diplomacy.PatchTools;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;

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
