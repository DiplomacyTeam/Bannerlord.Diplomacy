using Diplomacy.PatchTools;

using System;
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
            new Finalizer(nameof(GetUnitValueForFactionFinalizer), "GetUnitValueForFaction"),
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

        // Safety net: vanilla has other unguarded nulls in this method (e.g. a dead barterable
        // owner) that surface during the same kind of post-civil-war turmoil. Rather than chase
        // each one individually, swallow the NRE and treat the barter as worthless.
        private static Exception? GetUnitValueForFactionFinalizer(Exception? __exception, ref int __result)
        {
            if (__exception is NullReferenceException)
            {
                __result = 0;
                return null;
            }

            return __exception;
        }
    }
}
