using Diplomacy.PatchTools;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Diplomacy.Patches
{
    internal sealed class GiveGoldToClanActionPatch : PatchClass<GiveGoldToClanActionPatch>
    {
        private static readonly Type TargetType = typeof(GiveGoldToClanAction); // Static type

        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Prefix(nameof(ApplyFromHeroToClanReplaced), TargetType, nameof(GiveGoldToClanAction.ApplyFromHeroToClan)),
        };

        private delegate void ApplyInternalDel(Hero giverHero, Clan clan, int amount);
        private static readonly ApplyInternalDel ApplyInternal = new Reflect.Method(TargetType, "ApplyInternal").GetDelegate<ApplyInternalDel>();

        private static bool ApplyFromHeroToClanReplaced(Hero giverHero, Clan clan, int amount)
        {
            ApplyInternal(giverHero, clan, amount);
            return false;
        }
    }
}