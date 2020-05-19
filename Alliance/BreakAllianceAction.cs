using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace DiplomacyFixes.Alliance
{
    class BreakAllianceAction
    {
        public static void Apply(Kingdom kingdom, Kingdom otherKingdom)
        {
            Kingdom playerKingdom = Clan.PlayerClan?.Kingdom;
            if (otherKingdom == playerKingdom && playerKingdom.Leader == Hero.MainHero)
            {
                TextObject textObject = new TextObject("{=384jX28Q}{KINGDOM} is breaking the alliance with {PLAYER_KINGDOM}.");
                textObject.SetTextVariable("KINGDOM", kingdom.Name);
                textObject.SetTextVariable("PLAYER_KINGDOM", otherKingdom.Name);

                InformationManager.ShowInquiry(new InquiryData(
                    new TextObject("{=D1ZQKZr1}Alliance Broken").ToString(),
                    textObject.ToString(),
                    true,
                    false,
                    GameTexts.FindText("str_ok", null).ToString(),
                    null,
                    () => ApplyInternal(kingdom, otherKingdom),
                    null,
                    ""), true);
            }
            else
            {
                ApplyInternal(kingdom, otherKingdom);
            }
        }

        private static void ApplyInternal(Kingdom kingdom, Kingdom otherKingdom)
        {
            FactionManager.SetNeutral(kingdom, otherKingdom);
        }
    }
}
