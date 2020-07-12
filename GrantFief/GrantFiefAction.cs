using DiplomacyFixes.Extensions;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace DiplomacyFixes.GrantFief
{
    class GrantFiefAction
    {
        public static void Apply(Settlement settlement, Clan grantedClan)
        {
            ChangeOwnerOfSettlementAction.ApplyByDefault(grantedClan.Leader, settlement);

            int relationChange = CalculateBaseRelationChange(settlement);
            ChangeRelationAction.ApplyPlayerRelation(grantedClan.Leader, relationChange);

            foreach (Clan clan in Clan.PlayerClan.Kingdom.Clans.Where(clan => clan != grantedClan && clan != Clan.PlayerClan))
            {
                ChangeRelationAction.ApplyPlayerRelation(clan.Leader, Settings.Instance.GrantFiefRelationPenalty);
            }

            Events.Instance.OnFiefGranted(settlement.Town);
        }

        private static int CalculateBaseRelationChange(Settlement settlement)
        {
            int baseRelationChange = (int)Math.Round(Math.Max(5, Math.Log(settlement.Prosperity / 1000, 1.1f)));
            return (int)(baseRelationChange * Settings.Instance.GrantFiefPositiveRelationMultiplier);
        }

        public static int PreviewPositiveRelationChange(Settlement settlement, Hero hero)
        {
            int relationChange = CalculateBaseRelationChange(settlement);
            ExplainedNumber explainedNumber = new ExplainedNumber((float)relationChange, new StatExplainer(), null);
            Campaign.Current.Models.DiplomacyModel.GetRelationIncreaseFactor(Hero.MainHero, hero, ref explainedNumber);
            relationChange = (int)Math.Floor(explainedNumber.ResultNumber);
            return relationChange;
        }

        public static bool CanGrantFief(Clan targetClan, out string reason)
        {
            reason = null;
            if (targetClan == Clan.PlayerClan)
            {
                reason = new TextObject("{=FqeN0fmR}You cannot grant fiefs to your own clan.").ToString();
            }
            else if (targetClan.MapFaction.Leader != Hero.MainHero)
            {
                reason = new TextObject("{=zdSYUnZQ}You are not the leader of your kingdom.").ToString();
            }
            else if (Clan.PlayerClan.GetPermanentFiefs().Count() < 1)
            {
                reason = new TextObject("{=D61vzEC7}You don't have any fiefs to grant.").ToString();
            }

            return reason == null;
        }
    }
}
