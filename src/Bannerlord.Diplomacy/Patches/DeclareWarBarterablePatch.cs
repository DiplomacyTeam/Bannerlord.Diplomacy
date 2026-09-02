using Diplomacy.PatchTools;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;

namespace Diplomacy.Patches
{
    internal sealed class DeclareWarBarterablePatch : PatchClass<DeclareWarBarterablePatch, DeclareWarBarterable>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Prefix(nameof(GetUnitValueForFactionPrefix), "GetUnitValueForFaction"),
        };

        // Vanilla doesn't handle a Kingdom without a ruling clan (interregnum) here, causing a
        // NullReferenceException when GetScoreOfDeclaringWar is later called with a null clan.
        private static bool GetUnitValueForFactionPrefix(IFaction faction, ref int __result)
        {
            if (faction is Kingdom { RulingClan: null })
            {
                __result = 0;
                return false;
            }

            return true;
        }
    }
}
