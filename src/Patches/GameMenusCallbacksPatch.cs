using Diplomacy.PatchTools;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;

namespace Diplomacy.Patches
{
    internal sealed class GameMenusCallbacksPatch : PatchClass<GameMenusCallbacksPatch, GameMenusCallbacks>
    {
        protected override IEnumerable<Patch> Prepare()
        {
#if STABLE
            return new[]
            {
                new Postfix(nameof(menu_settlement_taken_on_init_Postfix), nameof(GameMenusCallbacks.menu_settlement_taken_on_init))
            };
#else
            // FIXME: Functionality is disabled until I can figure out how to replace this properly on e1.5.8
            // (menu_settlement_taken_on_init was removed)

            yield break;
#endif
        }

        private static void menu_settlement_taken_on_init_Postfix() => Events.Instance.OnPlayerSettlementTaken(Settlement.CurrentSettlement);
    }
}
