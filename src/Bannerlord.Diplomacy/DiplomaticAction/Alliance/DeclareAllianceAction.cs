using Diplomacy.Costs;
using Diplomacy.Events;

using Microsoft.Extensions.Logging;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Alliance
{
    internal sealed class DeclareAllianceAction : AbstractDiplomaticAction<DeclareAllianceAction>
    {
        private static readonly TextObject _TInquiry = new("{=QbOqatd7}{KINGDOM} is proposing an alliance with {PLAYER_KINGDOM}.");
        private static readonly TextObject _TAllianceProposal = new("{=3pbwc8sh}Alliance Proposal");

        public override bool PassesConditions(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCosts = false, bool bypassCosts = false)
            => FormAllianceConditions.Instance.CanApply(kingdom, kingdom, forcePlayerCosts, bypassCosts);

        protected override void ApplyInternal(Kingdom proposingKingdom, Kingdom otherKingdom, float? customDurationInDays)
        {
            LogFactory.Get<DeclareAllianceAction>().LogTrace($"[{CampaignTime.Now}] {proposingKingdom.Name} secured an alliance with {otherKingdom.Name}.");
            FactionManager.DeclareAlliance(proposingKingdom, otherKingdom);
            DiplomacyEvents.Instance.OnAllianceFormed(new AllianceEvent(proposingKingdom, otherKingdom));
        }

        protected override void AssessCosts(Kingdom proposingKingdom, Kingdom otherKingdom, bool forcePlayerCosts)
            => DiplomacyCostCalculator.DetermineCostForFormingAlliance(proposingKingdom, otherKingdom, forcePlayerCosts).ApplyCost();

        protected override void ShowPlayerInquiry(Kingdom proposingKingdom, Action acceptAction)
        {
            _TInquiry.SetTextVariable("KINGDOM", proposingKingdom.Name);
            _TInquiry.SetTextVariable("PLAYER_KINGDOM", Clan.PlayerClan.Kingdom.Name);

            InformationManager.ShowInquiry(new InquiryData(
                _TAllianceProposal.ToString(),
                _TInquiry.ToString(),
                true,
                true,
                new TextObject(StringConstants.Accept).ToString(),
                new TextObject(StringConstants.Decline).ToString(),
                acceptAction,
                null), true);
        }
    }
}