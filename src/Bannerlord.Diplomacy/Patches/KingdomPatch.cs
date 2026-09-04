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

        // Vanilla evaluates DetermineChooser().Leader.IsHumanPlayerCharacter without a null check, and
        // every DetermineChooser override returns Kingdom.RulingClan. KillCharacterAction adds the
        // king-selection decision *before* it installs a replacement ruling clan, so the chooser can be
        // a leaderless clan (or missing entirely) and the daily tick dies with a NullReferenceException.
        private static bool AddDecisionPrefix(KingdomDecision kingdomDecision)
        {
            try
            {
                if (kingdomDecision.DetermineChooser()?.Leader is not null)
                    return true;

                // Crown someone first so the decision has a valid chooser and can go to a vote as
                // usual. Only if even that fails is the decision dropped: unresolvable, and letting
                // it through would crash the tick.
                KingdomRulerRepairBehavior.EnsureRuler(kingdomDecision.Kingdom);
                return kingdomDecision.DetermineChooser()?.Leader is not null;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }
    }
}
