using Diplomacy.Extensions;
using Diplomacy.PatchTools;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.Encyclopedia.Pages;

namespace Diplomacy.Patches
{
    class DefaultEncyclopediaFactionPagePatch : PatchClass<DefaultEncyclopediaFactionPagePatch, DefaultEncyclopediaFactionPage>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Postfix(nameof(ApplyPostfix), nameof(DefaultEncyclopediaFactionPage.GetListItems)),
        };

        private static void ApplyPostfix(ref IEnumerable<EncyclopediaListItem>  __result, ref IEnumerable<EncyclopediaListItem>  ____items)
        {
            var listItems = new List<EncyclopediaListItem>(____items);
            foreach (EncyclopediaListItem item in ____items)
            {
                var kingdom = (Kingdom)item.Object;
                if (kingdom.IsRebelKingdom() && kingdom.IsEliminated)
                {
                    listItems.Remove(item);
                }
            }
            ____items = listItems;
            __result = ____items;
        }
    }
}
