using Diplomacy.Events;
using Diplomacy.PatchTools;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;

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

        private static void menu_settlement_taken_on_init_Postfix() => DiplomacyEvents.Instance.OnPlayerSettlementTaken(Settlement.CurrentSettlement);
    }
}