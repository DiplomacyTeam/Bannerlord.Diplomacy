using Diplomacy.CampaignBehaviors;
using Diplomacy.PatchTools;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;

namespace Diplomacy.Patches
{
    internal sealed class KingdomPatch : PatchClass<KingdomPatch, Kingdom>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Prefix(nameof(AddDecisionPrefix), "AddDecision"),
        };

        // AddDecision sets up the vote straight away, so state vanilla can't handle has to be repaired
        // here. A decision that still can't be evaluated is dropped, because letting it through would
        // crash the daily tick.
        private static bool AddDecisionPrefix(KingdomDecision kingdomDecision)
        {
            try
            {
                return HasChooser(kingdomDecision) && CanValueSettlements(kingdomDecision);
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        // Vanilla evaluates DetermineChooser().Leader.IsHumanPlayerCharacter without a null check, and
        // every DetermineChooser override returns Kingdom.RulingClan. KillCharacterAction adds the
        // king-selection decision *before* it installs a replacement ruling clan, so the chooser can be
        // a leaderless clan (or missing entirely).
        private static bool HasChooser(KingdomDecision kingdomDecision)
        {
            if (kingdomDecision.DetermineChooser()?.Leader is not null)
                return true;

            // Crown someone first so the decision has a valid chooser and can go to a vote as usual.
            KingdomRulerRepairBehavior.EnsureRuler(kingdomDecision.Kingdom);
            return kingdomDecision.DetermineChooser()?.Leader is not null;
        }

        // The settlement claimant vote values the fief for every candidate clan, which needs each clan's
        // mid-settlement. MidSettlementRepairBehavior keeps those set; this is the backstop for a clan
        // that reached the kingdom without raising an event.
        private static bool CanValueSettlements(KingdomDecision kingdomDecision)
        {
            var allValued = true;

            foreach (var clan in kingdomDecision.Kingdom.Clans)
                allValued &= MidSettlementRepairBehavior.EnsureMidSettlement(clan);

            // Other decisions value fiefs for the kingdom as a whole, not for a clan, so they go ahead.
            return allValued || kingdomDecision is not SettlementClaimantDecision;
        }
    }
}
