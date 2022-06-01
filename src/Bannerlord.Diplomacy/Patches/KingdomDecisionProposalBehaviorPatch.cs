using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.Extensions;
using Diplomacy.PatchTools;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Election;

namespace Diplomacy.Patches
{
    internal sealed class KingdomDecisionProposalBehaviorPatch : PatchClass<KingdomDecisionProposalBehaviorPatch, KingdomDecisionProposalBehavior>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Prefix(nameof(ConsiderWarPrefix), "ConsiderWar"),
            new Prefix(nameof(ConsiderPeacePrefix), "ConsiderPeace"),
            new Prefix(nameof(HandleRebelKingdom), "DailyTickClan")
        };

        private static bool HandleRebelKingdom(Clan clan)
        {
            // rebel kingdoms don't make decisions
            if (clan.Kingdom?.IsRebelKingdom() ?? false)
                return false;

            return true;
        }

        private static bool ConsiderWarPrefix(Clan clan, Kingdom kingdom, IFaction otherFaction, ref bool __result)
        {
            if (otherFaction is Kingdom otherKingdom
                && !DeclareWarConditions.Instance.CanApply(kingdom, otherKingdom, bypassCosts: true))
            {
                __result = false;
                return false;
            }

            return true;
        }

        private static bool ConsiderPeacePrefix(Clan clan, Clan otherClan, Kingdom kingdom, IFaction otherFaction, out MakePeaceKingdomDecision? decision, ref bool __result)
        {
            decision = null;

            if (otherFaction is Kingdom otherKingdom
                && !MakePeaceConditions.Instance.CanApply(kingdom, otherKingdom, bypassCosts: true))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}