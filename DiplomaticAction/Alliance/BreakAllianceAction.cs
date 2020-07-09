using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.Alliance
{
    class BreakAllianceAction : AbstractDiplomaticAction<BreakAllianceAction>
    {
        public override bool PassesConditions(Kingdom proposingKingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            return BreakAllianceConditions.Instance.CanApply(proposingKingdom, otherKingdom, forcePlayerCharacterCosts, bypassCosts);
        }

        protected override void ApplyInternal(Kingdom proposingKingdom, Kingdom otherKingdom)
        {
            FactionManager.SetNeutral(proposingKingdom, otherKingdom);
            Events.Instance.OnAllianceBroken(new AllianceEvent(proposingKingdom, otherKingdom));
        }

        protected override void AssessCosts(Kingdom proposingKingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts)
        {
        }

        protected override void ShowPlayerInquiry(Kingdom proposingKingdom, Action acceptAction)
        {
            TextObject textObject = new TextObject("{=384jX28Q}{KINGDOM} is breaking the alliance with {PLAYER_KINGDOM}.");
            textObject.SetTextVariable("KINGDOM", proposingKingdom.Name);
            textObject.SetTextVariable("PLAYER_KINGDOM", Clan.PlayerClan.Kingdom.Name);

            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=D1ZQKZr1}Alliance Broken").ToString(),
                textObject.ToString(),
                true,
                false,
                GameTexts.FindText("str_ok", null).ToString(),
                null,
                acceptAction,
                null,
                ""), true);
        }
    }
}
