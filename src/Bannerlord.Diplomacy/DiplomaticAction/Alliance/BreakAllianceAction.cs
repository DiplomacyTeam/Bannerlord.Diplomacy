using Diplomacy.Events;

using Microsoft.Extensions.Logging;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Alliance
{
    class BreakAllianceAction : AbstractDiplomaticAction<BreakAllianceAction>
    {
        public override bool PassesConditions(Kingdom proposingKingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            return BreakAllianceConditions.Instance.CanApply(proposingKingdom, otherKingdom, forcePlayerCharacterCosts, bypassCosts);
        }

        protected override void ApplyInternal(Kingdom proposingKingdom, Kingdom otherKingdom, float? customDurationInDays)
        {
            LogFactory.Get<BreakAllianceAction>().LogTrace($"[{CampaignTime.Now}] {proposingKingdom.Name} broke their alliance with {otherKingdom.Name}.");
            FactionManager.SetNeutral(proposingKingdom, otherKingdom);
            DiplomacyEvents.Instance.OnAllianceBroken(new AllianceEvent(proposingKingdom, otherKingdom));
        }

        protected override void AssessCosts(Kingdom proposingKingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts)
        {
        }

        protected override void ShowPlayerInquiry(Kingdom proposingKingdom, Action acceptAction)
        {
            var textObject = new TextObject("{=384jX28Q}{KINGDOM} is breaking the alliance with {PLAYER_KINGDOM}.");
            textObject.SetTextVariable("KINGDOM", proposingKingdom.Name);
            textObject.SetTextVariable("PLAYER_KINGDOM", Clan.PlayerClan.Kingdom.Name);

            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=D1ZQKZr1}Alliance Broken").ToString(),
                textObject.ToString(),
                true,
                false,
                GameTexts.FindText("str_ok").ToString(),
                null,
                acceptAction,
                null), true);
        }
    }
}