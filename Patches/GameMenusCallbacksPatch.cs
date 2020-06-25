using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;

namespace DiplomacyFixes.Patches
{
    [HarmonyPatch(typeof(GameMenusCallbacks))]
    class GameMenusCallbacksPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("menu_settlement_taken_on_init")]
        public static void menu_settlement_taken_on_initPatch()
        {
            Events.Instance.OnPlayerSettlementTaken(Settlement.CurrentSettlement);
        }
    }
}
