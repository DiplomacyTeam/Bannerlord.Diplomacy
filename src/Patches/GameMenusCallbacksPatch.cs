using Diplomacy.PatchTools;
using HarmonyLib;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace Diplomacy.Patches
{
    internal sealed class GameMenusCallbacksPatch : PatchClass<GameMenusCallbacksPatch>
    {
        protected override IEnumerable<Patch> Prepare()
        {
#if STABLE
            return new[]
            {
                new Postfix(nameof(menu_settlement_taken_on_init_Postfix), typeof(GameMenusCallbacks), nameof(GameMenusCallbacks.menu_settlement_taken_on_init))
            };
#else
            // FIXME: Functionality is disabled until I can figure out how to replace this properly on e1.5.8
            // var method = AccessTools.Method(typeof(SiegeAftermathCampaignBehavior), "siege_aftermath_contextual_summary_on_init").Name;
            return new[]
            {
                new Postfix(nameof(menu_settlement_taken_on_init_Postfix), typeof(SiegeAftermathCampaignBehavior), "siege_aftermath_contextual_summary_on_init")
            };
#endif
        }

        private static void menu_settlement_taken_on_init_Postfix() => Events.Instance.OnPlayerSettlementTaken(Settlement.CurrentSettlement);
    }
}
