using Diplomacy.PatchTools;
using Diplomacy.ViewModel;

using HarmonyLib;

using SandBox.GauntletUI;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;

namespace Diplomacy.Patches
{
    internal sealed class EncyclopediaDataPatch : PatchClass<EncyclopediaDataPatch, EncyclopediaData>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Postfix(nameof(GetEncyclopediaPageInstancePostfix), "GetEncyclopediaPageInstance")
        };

        public static void GetEncyclopediaPageInstancePostfix(ref EncyclopediaPageVM __result)
        {
            var args = (EncyclopediaPageArgs) AccessTools.Field(typeof(EncyclopediaPageVM), "_args").GetValue(__result);

            if (__result is EncyclopediaHeroPageVM)
                __result = new EncyclopediaHeroPageVMExtensionVM(args);
            else if (__result is EncyclopediaFactionPageVM)
                __result = new EncyclopediaFactionPageVMExtensionVM(args);
        }
    }
}