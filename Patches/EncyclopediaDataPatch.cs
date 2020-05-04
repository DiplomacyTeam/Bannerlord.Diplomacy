using DiplomacyFixes.ViewModel;
using HarmonyLib;
using SandBox.GauntletUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;

namespace DiplomacyFixes.Patches
{
    [HarmonyPatch(typeof(EncyclopediaData))]
    class EncyclopediaDataPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("GetEncyclopediaPageInstance")]
        public static void GetEncyclopediaPageInstancePatch(ref EncyclopediaPageVM __result)
        {
            if (__result is EncyclopediaHeroPageVM)
            {
                EncyclopediaPageArgs args = (EncyclopediaPageArgs) typeof(EncyclopediaPageVM).GetField("_args", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__result);
                __result = new EncyclopediaHeroPageVMExtensionVM(args);
            }
        }
    }
}
