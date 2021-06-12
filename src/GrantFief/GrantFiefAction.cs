using Diplomacy.Character;
using Diplomacy.Event;
using Diplomacy.Extensions;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace Diplomacy.GrantFief
{
    internal sealed class GrantFiefAction
    {
        public static void Apply(Settlement settlement, Clan grantedClan)
        {
            ChangeOwnerOfSettlementAction.ApplyByDefault(grantedClan.Leader, settlement);

            var relationChange = CalculateBaseRelationChange(settlement);
            ChangeRelationAction.ApplyPlayerRelation(grantedClan.Leader, relationChange);

            foreach (var clan in Clan.PlayerClan.Kingdom.Clans.Where(c => c != grantedClan && c != Clan.PlayerClan))
                ChangeRelationAction.ApplyPlayerRelation(clan.Leader, Settings.Instance!.GrantFiefRelationPenalty);

            // gain generosity when granting fief
            PlayerCharacterTraitEventExperience.FiefGranted.Apply();

            Events.Instance.OnFiefGranted(settlement.Town);
        }

        private static int CalculateBaseRelationChange(Settlement settlement)
        {
            // TODO: Consider basing the relationship change with the granted clan upon the fief's value
            // normalized to the average fief value in the kingdom.
            var baseRelationChange = (int)Math.Round(Math.Max(5, Math.Log(settlement.Prosperity / 1000, 1.1f)));
            return (int)(baseRelationChange * Settings.Instance!.GrantFiefPositiveRelationMultiplier);
        }

        public static int PreviewPositiveRelationChange(Settlement settlement, Hero hero)
        {
            // This thing had a non-null StatExplainer passed into an ExplainedNumber previously, but the
            // ExplainedNumber (and the StatExplainer) never ended up leaving this method, so in fixing e1.5.7
            // API compatibility, I simply removed them. But it's strange that this method is prefixed with
            // Preview as if you were going to see a breakdown.
            int relationChange = CalculateBaseRelationChange(settlement);
            float adjustedChange = Campaign.Current.Models.DiplomacyModel.GetRelationIncreaseFactor(Hero.MainHero, hero, relationChange);
            return (int)Math.Floor(adjustedChange);
        }

        public static bool CanGrantFief(Clan targetClan, out string? reason)
        {
            reason = null;

            if (targetClan == Clan.PlayerClan)
                reason = _TNoGrantsToMyClan.ToString();
            else if (targetClan.MapFaction.Leader != Hero.MainHero)
                reason = _TNotKingdomLeader.ToString();
            else if (Clan.PlayerClan.GetPermanentFiefs().Count() < 1)
                reason = _TNoFiefsToGrant.ToString();

            return reason is null;
        }

        private static readonly TextObject _TNoGrantsToMyClan = new TextObject("{=FqeN0fmR}You cannot grant fiefs to your own clan.");
        private static readonly TextObject _TNotKingdomLeader = new TextObject("{=zdSYUnZQ}You are not the leader of your kingdom.");
        private static readonly TextObject _TNoFiefsToGrant = new TextObject("{=D61vzEC7}You don't have any fiefs to grant.");
    }
}
