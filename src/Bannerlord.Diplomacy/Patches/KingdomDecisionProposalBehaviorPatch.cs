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
            new Prefix(nameof(HandleRebelKingdom), "DailyTickClan"),
            new Prefix(nameof(ConsiderTradeAgreementPrefix), "ConsiderTradeAgreement"),
        };

        // GetRandomTradeAgreementDecision picks its candidate with
        // Kingdom.All.GetRandomElementWithPredicate(x => x != kingdom) — every sibling method in this
        // behavior also filters !x.IsEliminated, and this one doesn't. Destroyed kingdoms stay in
        // Kingdom.All with no ruling clan, so scoring the agreement against one dereferences a null
        // RulingClan/Leader all through DefaultTradeAgreementModel.
        private static bool ConsiderTradeAgreementPrefix(Kingdom kingdom, Kingdom otherKingdom, ref bool __result)
        {
            if (otherKingdom.IsEliminated || otherKingdom.RulingClan?.Leader is null
                || kingdom.IsEliminated || kingdom.RulingClan?.Leader is null)
            {
                __result = false;
                return false;
            }

            return true;
        }

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

        private static bool ConsiderPeacePrefix(Clan clan, Clan otherClan, IFaction otherFaction, out MakePeaceKingdomDecision? decision, ref bool __result)
        {
            decision = null;

            // GetRandomPeaceDecision picks its candidate by war state alone, and stances outlive the
            // kingdom, so a destroyed (ruler-less) kingdom can come back from that draw.
            if (otherFaction is Kingdom { IsEliminated: true } or Kingdom { RulingClan: null })
            {
                __result = false;
                return false;
            }

            if (otherFaction is Kingdom otherKingdom
                && !MakePeaceConditions.Instance.CanApply(clan.Kingdom, otherKingdom, bypassCosts: true))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}