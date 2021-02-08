using Diplomacy.PatchTools;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.BarterBehaviors;

namespace Diplomacy.Patches
{
    /// <summary>
    /// Blocks AI from declaring war due to the AI diplomatic barter behavior.
    /// The other way for the AI to consider war is via a kingdom decision proposal.
    /// </summary>
    internal sealed class DiplomaticBartersBehaviorPatch : PatchClass<DiplomaticBartersBehaviorPatch, DiplomaticBartersBehavior>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Prefix(nameof(ConsiderWarPrefix), "ConsiderWar"),
        };

        private static bool ConsiderWarPrefix(Clan clan, IFaction otherMapFaction)
            => !CooldownManager.HasDeclareWarCooldown(clan, otherMapFaction, out _);
    }
}
