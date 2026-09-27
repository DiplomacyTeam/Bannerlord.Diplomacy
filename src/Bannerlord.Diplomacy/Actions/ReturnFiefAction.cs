using Diplomacy.Character;
using Diplomacy.Events;
using Diplomacy.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace Diplomacy.Actions
{
    /// <summary>
    /// Hands a conquered fief back to the kingdom it was taken from. The relation gain is the
    /// flavour; the payout that actually changes how the AI treats you is the drop in
    /// expansionism, which feeds the coalition trigger and every diplomatic score.
    /// </summary>
    internal sealed class ReturnFiefAction
    {
        private static readonly TextObject _TNotKingdomLeader = new("{=zdSYUnZQ}You are not the leader of your kingdom.");
        private static readonly TextObject _TDisabled = new("{=8kQrWvN2}Returning fiefs has been disabled.");
        private static readonly TextObject _TNotYourFief = new("{=Yj4mLpQd}You can only return fiefs held by your own clan.");
        private static readonly TextObject _TSameKingdom = new("{=Tq7wBnXe}You cannot return a fief to your own kingdom.");
        private static readonly TextObject _TKingdomGone = new("{=Vc2rHkZs}That kingdom no longer exists.");
        private static readonly TextObject _TRebelKingdom = new("{=Nd6pXtLw}You cannot return fiefs to a rebel kingdom.");
        private static readonly TextObject _TNotConquered = new("{=Kb9sRmTy}{SETTLEMENT} was not taken from {KINGDOM}.");
        private static readonly TextObject _TUnderSiege = new("{=Qs3vLpXa}{SETTLEMENT} is under siege and cannot be handed over.");
        private static readonly TextObject _TOnCooldown = new("{=Gm5tRvWc}{SETTLEMENT} was handed back too recently. {DAYS} more days must pass.");

        /// <summary>The share of the payout that the receiving kingdom's other clans feel.</summary>
        private const float _bystanderRelationShare = 0.35f;

        /// <summary>Damping applied per additional time a fief has passed between kingdoms.</summary>
        private const float _churnPenaltyPerFlip = 0.25f;

        public static void Apply(Settlement settlement, Kingdom targetKingdom)
        {
            var recipientClan = FiefProvenanceManager.Instance!.GetDispossessedClan(settlement, targetKingdom) ?? targetKingdom.Leader.Clan;
            var relationChange = CalculateRelationChange(settlement);

            // Captured before the transfer, because afterwards the fief belongs to someone else.
            var relinquishingKingdom = settlement.OwnerClan?.Kingdom;

            ChangeOwnerOfSettlementAction.ApplyByLeaveFaction(recipientClan.Leader, settlement);

            ChangeRelationAction.ApplyPlayerRelation(recipientClan.Leader, relationChange);

            var bystanderChange = (int) Math.Round(relationChange * _bystanderRelationShare);
            if (bystanderChange > 0)
            {
                foreach (var clan in targetKingdom.Clans.Where(clan => clan != recipientClan
                                                                       && clan.Leader is not null
                                                                       && !clan.IsUnderMercenaryService))
                    ChangeRelationAction.ApplyPlayerRelation(clan.Leader, bystanderChange);
            }

            // The fief is gone, so the kingdom's minimum expansionism has already fallen with it.
            // This gives back the score the siege that took it added on top.
            if (relinquishingKingdom is not null)
                ExpansionismManager.Instance!.ReduceExpansionism(relinquishingKingdom, Settings.Instance!.ReturnFiefExpansionismReduction);

            PlayerCharacterTraitEventExperience.FiefReturned.Apply();

            DiplomacyEvents.Instance.OnFiefReturned(settlement.Town);
        }

        public static int PreviewRelationChange(Settlement settlement, Kingdom targetKingdom)
        {
            var recipient = FiefProvenanceManager.Instance!.GetDispossessedClan(settlement, targetKingdom)?.Leader ?? targetKingdom.Leader;
            var relationChange = CalculateRelationChange(settlement);
#if BL15
            var adjustedChange = Campaign.Current.Models.DiplomacyModel.GetEffectiveRelationChange(Hero.MainHero, recipient, relationChange);
#else
            var adjustedChange = Campaign.Current.Models.DiplomacyModel.GetRelationIncreaseFactor(Hero.MainHero, recipient, relationChange);
#endif
            return (int) Math.Floor((double) adjustedChange);
        }

        /// <summary>The fiefs of the player's own clan that were taken from the given kingdom.</summary>
        public static IEnumerable<Settlement> GetReturnableFiefs(Kingdom targetKingdom) =>
            Clan.PlayerClan.GetPermanentFiefs()
                .Select(town => town.Settlement)
                .Where(settlement => CanReturnFief(settlement, targetKingdom, out _));

        public static bool CanReturnFief(Settlement settlement, Kingdom targetKingdom, out string? reason)
        {
            reason = null;

            if (!Settings.Instance!.EnableFiefReturn)
                reason = _TDisabled.ToString();
            else if (settlement.OwnerClan != Clan.PlayerClan)
                reason = _TNotYourFief.ToString();
            else if (Clan.PlayerClan.MapFaction?.Leader != Hero.MainHero)
                reason = _TNotKingdomLeader.ToString();
            else if (targetKingdom == Clan.PlayerClan.Kingdom)
                reason = _TSameKingdom.ToString();
            else if (targetKingdom.IsEliminated || targetKingdom.Leader is null)
                reason = _TKingdomGone.ToString();
            else if (targetKingdom.IsRebelKingdom())
                reason = _TRebelKingdom.ToString();
            else if (settlement.IsUnderSiege)
            {
                // Handing over a fief mid-siege would leave the besiegers attacking a kingdom they may be at peace with.
                _TUnderSiege.SetTextVariable("SETTLEMENT", settlement.Name);
                reason = _TUnderSiege.ToString();
            }
            else if (CooldownManager.HasFiefReturnCooldown(settlement, out var elapsedDays))
            {
                _TOnCooldown.SetTextVariable("SETTLEMENT", settlement.Name);
                _TOnCooldown.SetTextVariable("DAYS", (int) Math.Ceiling(Settings.Instance!.ReturnFiefCooldownInDays - elapsedDays));
                reason = _TOnCooldown.ToString();
            }
            else if (!(FiefProvenanceManager.Instance?.WasConqueredFrom(settlement, targetKingdom, out _) ?? false))
            {
                _TNotConquered.SetTextVariable("SETTLEMENT", settlement.Name);
                _TNotConquered.SetTextVariable("KINGDOM", targetKingdom.Name);
                reason = _TNotConquered.ToString();
            }

            return reason is null;
        }

        private static int CalculateRelationChange(Settlement settlement)
        {
            var baseChange = Math.Max(5d, Math.Log(settlement.Town.Prosperity / 1000, 1.1f));
            var scaled = baseChange
                         * Settings.Instance!.ReturnFiefRelationMultiplier
                         * GetHoldFactor(settlement)
                         * GetChurnFactor(settlement);

            return Math.Max(1, (int) Math.Round(scaled));
        }

        /// <summary>
        /// A fief handed straight back is barely a sacrifice, and returning one the moment it is
        /// taken would otherwise be farmable. Value ramps to full over a year of ownership.
        /// </summary>
        private static float GetHoldFactor(Settlement settlement)
        {
            var heldSince = FiefProvenanceManager.Instance!.GetHeldByCurrentKingdomSince(settlement);
            return heldSince == CampaignTime.Zero
                ? 1f
                : Math.Min(1f, heldSince.ElapsedDaysUntilNow / CampaignTime.DaysInYear);
        }

        /// <summary>Damps the payout for fiefs that have been passed back and forth.</summary>
        private static float GetChurnFactor(Settlement settlement)
        {
            var flips = FiefProvenanceManager.Instance!.GetTimesChangedHands(settlement);
            return 1f / (1f + (_churnPenaltyPerFlip * Math.Max(0, flips - 1)));
        }
    }
}
