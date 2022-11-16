using Diplomacy.PatchTools;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;

namespace Diplomacy.Patches
{
    internal sealed class FactionManagerPatch : PatchClass<FactionManagerPatch, FactionManager>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Prefix(nameof(DeclareAlliancePrefix), nameof(FactionManager.DeclareAlliance)),
        };

        private static bool DeclareAlliancePrefix(IFaction faction1, IFaction faction2)
        {
            if (faction1 == faction2 || faction1.IsBanditFaction || faction2.IsBanditFaction)
                return false;

            SetStance(faction1, faction2, StanceType.Alliance);
            return false;
        }

        private enum StanceType
        {
            [UsedImplicitly] Neutral,
            [UsedImplicitly] War,
            Alliance,
        }

        private static readonly Func<IFaction, IFaction, StanceType, StanceLink> SetStance = new Reflect.Method<FactionManager>("SetStance").GetDelegate<Func<IFaction, IFaction, StanceType, StanceLink>>();
    }
}