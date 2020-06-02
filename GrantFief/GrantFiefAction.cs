using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace DiplomacyFixes.GrantFief
{
    class GrantFiefAction
    {
        public static void Apply(Settlement settlement, Clan clan)
        {
            ChangeOwnerOfSettlementAction.ApplyByDefault(clan.Leader, settlement);

            int relationChange = CalculateBaseRelationChange(settlement);
            ChangeRelationAction.ApplyPlayerRelation(clan.Leader, relationChange);

            Events.Instance.OnFiefGranted(settlement.Town);
        }

        private static int CalculateBaseRelationChange(Settlement settlement)
        {
            return (int)Math.Round(Math.Max(5, Math.Log(settlement.Prosperity / 1000, 1.1f)));
        }

        public static int PreviewRelationChange(Settlement settlement, Hero hero)
        {
            int relationChange = CalculateBaseRelationChange(settlement);
            ExplainedNumber explainedNumber = new ExplainedNumber((float)relationChange, new StatExplainer(), null);
            Campaign.Current.Models.DiplomacyModel.GetRelationIncreaseFactor(Hero.MainHero, hero, ref explainedNumber);
            relationChange = (int) Math.Floor(explainedNumber.ResultNumber);
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
            else if (Clan.PlayerClan.Fortifications.Count < 1)
            {
                reason = new TextObject("{=D61vzEC7}You don't have any fiefs to grant.").ToString();
            }

            return reason == null;
        }
    }
}
