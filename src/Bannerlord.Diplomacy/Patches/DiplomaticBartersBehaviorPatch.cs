using Diplomacy.Extensions;
using Diplomacy.PatchTools;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors.BarterBehaviors;

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
            new Prefix(nameof(ConsiderClanLeaveKingdomPrefix), "ConsiderClanLeaveKingdom"),
            new Prefix(nameof(ConsiderDefectionPrefix), "ConsiderDefection"),
        };

        // Vanilla's GetScoreOfClanToLeaveKingdom/GetRelationBetweenClans doesn't handle a kingdom
        // without a ruling clan (interregnum), causing a NullReferenceException.
        private static bool ConsiderClanLeaveKingdomPrefix(Clan clan)
        {
            return clan.Kingdom?.RulingClan != null;
        }

        // The same vanilla bug, reached through the defection path: ConsiderDefection evaluates a
        // JoinKingdomAsClanBarterable, which chains into LeaveKingdomAsClanBarterable and
        // GetScoreOfClanToLeaveKingdom. That one reads Kingdom.RulingClan unguarded and hands it to
        // FactionManager.GetRelationBetweenClans, so a kingdom in interregnum throws a
        // NullReferenceException.
        private static bool ConsiderDefectionPrefix(Clan clan1, Kingdom kingdom)
        {
            if (kingdom.RulingClan is null)
                return false;

            // A clanless clan never evaluates leaving, so only block when it has a ruler-less kingdom.
            if (clan1.Kingdom is { RulingClan: null })
                return false;

            return true;
        }

        private static bool ConsiderWarPrefix(Clan clan, IFaction otherMapFaction)
        {
            // if the opponent is a rebel kingdom
            if ((otherMapFaction as Kingdom)?.IsRebelKingdom() ?? false)
            {
                return false;
            }

            // if this clan is in a rebel kingdom
            if (clan.Kingdom?.IsRebelKingdom() ?? false)
            {
                return false;
            }

            // enforce cooldowns
            if (CooldownManager.HasDeclareWarCooldown(clan, otherMapFaction, out _))
            {
                return false;
            }

            return true;
        }
    }
}