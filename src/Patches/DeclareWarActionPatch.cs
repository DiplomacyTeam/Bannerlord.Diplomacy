using Diplomacy.DiplomaticAction.WarPeace;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Diplomacy.Patches
{
    [HarmonyPatch(typeof(DeclareWarAction))]
    class DeclareWarActionPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Apply")]
        public static void ApplyPatch(IFaction faction1, IFaction faction2)
        {
            Events.Instance.OnWarDeclared(new WarDeclaredEvent(faction1, faction2, false));
        }

        [HarmonyPostfix]
        [HarmonyPatch("ApplyDeclareWarOverProvocation")]
        public static void ApplyDeclareWarOverProvocationPatch(IFaction faction, IFaction provocatorFaction)
        {
            Events.Instance.OnWarDeclared(new WarDeclaredEvent(faction, provocatorFaction, true));
        }
    }
}
