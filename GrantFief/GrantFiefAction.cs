using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace DiplomacyFixes.GrantFief
{
    class GrantFiefAction
    {
        public static void Apply(Settlement settlement, Clan clan)
        {
            ChangeOwnerOfSettlementAction.ApplyByDefault(clan.Leader, settlement);
            Events.Instance.OnFiefGranted(settlement.Town);
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
