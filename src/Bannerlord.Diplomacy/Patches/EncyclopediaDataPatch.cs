using Diplomacy.PatchTools;
using Diplomacy.ViewModel;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using SandBox.GauntletUI.Encyclopedia;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;

namespace Diplomacy.Patches
{
    internal sealed class EncyclopediaDataPatch : PatchClass<EncyclopediaDataPatch, EncyclopediaData>
    {
        private static readonly AccessTools.FieldRef<EncyclopediaPageVM, EncyclopediaPageArgs>? _args =
            AccessTools2.FieldRefAccess<EncyclopediaPageVM, EncyclopediaPageArgs>("_args");

        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Postfix(nameof(GetEncyclopediaPageInstancePostfix), "GetEncyclopediaPageInstance")
        };

        public static void GetEncyclopediaPageInstancePostfix(ref EncyclopediaPageVM __result)
        {
            if (_args?.Invoke(__result) is { } args && __result is EncyclopediaFactionPageVM)
                __result = new EncyclopediaFactionPageVMExtensionVM(args);
        }
    }
}