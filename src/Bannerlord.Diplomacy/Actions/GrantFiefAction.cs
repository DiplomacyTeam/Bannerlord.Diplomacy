using Diplomacy.Character;
using Diplomacy.Events;
using Diplomacy.Extensions;

using System;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace Diplomacy.Actions
{
    internal sealed class GrantFiefAction
    {
        private static readonly TextObject _TNoGrantsToMyClan = new("{=FqeN0fmR}You cannot grant fiefs to your own clan.");
        private static readonly TextObject _TNotKingdomLeader = new("{=zdSYUnZQ}You are not the leader of your kingdom.");
        private static readonly TextObject _TNoFiefsToGrant = new("{=D61vzEC7}You don't have any fiefs to grant.");
        private static readonly TextObject _TNoGrantsToMercenaries = new("{=Q7jRqnez}You cannot grant fiefs to mercenary clans.");

        private static readonly float _noJealousyMultiplier = 1.2f;

        public static void Apply(Settlement settlement, Clan grantedClan)
        {
            var grantedClanFiefCount = grantedClan.Fiefs.Count;
            var grantedClanProsperity = grantedClan.Fiefs.Sum(f => f.Prosperity);

            ChangeOwnerOfSettlementAction.ApplyByDefault(grantedClan.Leader, settlement);

            var relationChange = CalculateBaseRelationChange(settlement);
            ChangeRelationAction.ApplyPlayerRelation(grantedClan.Leader, relationChange);

            foreach (var clan in Clan.PlayerClan.Kingdom.Clans.Where(c => ShouldNegativeRelationBeApplied(c, grantedClan, grantedClanFiefCount, grantedClanProsperity)))
                ChangeRelationAction.ApplyPlayerRelation(clan.Leader, Settings.Instance!.GrantFiefRelationPenalty);

            // gain generosity when granting fief
            PlayerCharacterTraitEventExperience.FiefGranted.Apply();

            DiplomacyEvents.Instance.OnFiefGranted(settlement.Town);
        }

        private static bool ShouldNegativeRelationBeApplied(Clan clan, Clan grantedClan, int grantedClanFiefCount, float grantedClanProsperity)
        {
            if (clan == grantedClan || clan == Clan.PlayerClan || clan.IsUnderMercenaryService)
                return false;
            return clan.Fiefs.Count <= grantedClanFiefCount * _noJealousyMultiplier || clan.Fiefs.Sum(f => f.Prosperity) <= grantedClanProsperity * _noJealousyMultiplier;
        }

        private static int CalculateBaseRelationChange(Settlement settlement)
        {
            // TODO: Consider basing the relationship change with the granted clan upon the fief's value
            // normalized to the average fief value in the kingdom.
#if v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115 || v116
            var baseRelationChange = (int) Math.Round(Math.Max(5, Math.Log(settlement.Prosperity / 1000, 1.1f)));
#else
            var baseRelationChange = (int) Math.Round(Math.Max(5, Math.Log(settlement.Town.Prosperity / 1000, 1.1f)));
#endif
            return (int) (baseRelationChange * Settings.Instance!.GrantFiefPositiveRelationMultiplier);
        }

        public static int PreviewPositiveRelationChange(Settlement settlement, Hero hero)
        {
            // This thing had a non-null StatExplainer passed into an ExplainedNumber previously, but the
            // ExplainedNumber (and the StatExplainer) never ended up leaving this method, so in fixing e1.5.7
            // API compatibility, I simply removed them. But it's strange that this method is prefixed with
            // Preview as if you were going to see a breakdown.
            var relationChange = CalculateBaseRelationChange(settlement);
            var adjustedChange = Campaign.Current.Models.DiplomacyModel.GetRelationIncreaseFactor(Hero.MainHero, hero, relationChange);
            return (int) Math.Floor(adjustedChange);
        }

        public static bool CanGrantFief(Clan targetClan, out string? reason)
        {
            reason = null;

            if (targetClan == Clan.PlayerClan)
                reason = _TNoGrantsToMyClan.ToString();
            else if (targetClan.MapFaction.Leader != Hero.MainHero)
                reason = _TNotKingdomLeader.ToString();
            else if (!Clan.PlayerClan.GetPermanentFiefs().Any())
                reason = _TNoFiefsToGrant.ToString();
            else if (targetClan.IsMinorFaction || targetClan.IsUnderMercenaryService)
                reason = _TNoGrantsToMercenaries.ToString();

            return reason is null;
        }
    }
}