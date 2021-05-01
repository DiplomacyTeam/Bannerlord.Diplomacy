using Diplomacy.Event;
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
            return new[]
            {
                new Postfix(nameof(menu_settlement_taken_on_init_Postfix), typeof(SiegeAftermathCampaignBehavior), "siege_aftermath_contextual_summary_on_init")
            };
        }

        private static void menu_settlement_taken_on_init_Postfix() => Events.Instance.OnPlayerSettlementTaken(Settlement.CurrentSettlement);
    }
}
