using Diplomacy.Extensions;
using Diplomacy.PatchTools;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.Encyclopedia.Pages;

namespace Diplomacy.Patches
{
    internal sealed class DefaultEncyclopediaFactionPagePatch : PatchClass<DefaultEncyclopediaFactionPagePatch, DefaultEncyclopediaFactionPage>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Postfix(nameof(PassThroughPostfix), "InitializeListItems"),
        };

        // ReSharper disable once RedundantAssignment
        private static IEnumerable<EncyclopediaListItem> PassThroughPostfix(IEnumerable<EncyclopediaListItem> values)
        {
            foreach (var item in values.ToList())
            {
                var kingdom = (Kingdom) item.Object;
                if (kingdom.IsRebelKingdom() && kingdom.IsEliminated)
                {
                    continue;
                }
                yield return item;
            }
        }
    }
}