using Diplomacy.Costs;
using Diplomacy.Event;
using Diplomacy.Extensions;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Alliance
{
    internal sealed class DeclareAllianceAction : AbstractDiplomaticAction<DeclareAllianceAction>
    {
        private static readonly TextObject _TInquiry = new("{=QbOqatd7}{KINGDOM} is proposing an alliance with {PLAYER_KINGDOM}.");
        private static readonly TextObject _TIvolvedInquiry = new("{=tjtDZNWq}Kingdom {KINGDOM}{?INVOLVED_WITH_PROPOSER}, allied to kingdom {PLAYER_KINGDOM}, proposes an alliance with {OTHER_KINGDOM}{?} proposes an alliance with {OTHER_KINGDOM}, which is an ally of {PLAYER_KINGDOM}{//?}.{NEW_LINE} {NEW_LINE}As an active member of the existing alliance, you must vote whether or not to accept the new kingdom in it. The treaty will be approved only if all the parties involved agree with it.");
        private static readonly TextObject _TAllianceProposal = new("{=3pbwc8sh}Alliance Proposal");

        protected override bool JointResolutionRequired => true;

        public override bool PassesConditions(Kingdom kingdom, Kingdom otherKingdom, DiplomacyConditionType accountedConditions = DiplomacyConditionType.All, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer) =>
            FormAllianceConditions.Instance.CanApply(kingdom, otherKingdom, accountedConditions, forcePlayerCharacterCosts, kingdomPartyType);        

        protected override void ApplyInternal(Kingdom kingdom, Kingdom otherKingdom, float? customDurationInDays)
        {
            if (kingdom.IsAlliedWith(otherKingdom))
            {
                LogFactory.Get<DeclareAllianceAction>().LogTrace($"[{CampaignTime.Now}] {kingdom.Name} secured an alliance with {otherKingdom.Name}.");
                FactionManager.DeclareAlliance(kingdom, otherKingdom);
                Events.Instance.OnAllianceFormed(new AllianceEvent(kingdom, otherKingdom));
            }
        }

        protected override void AssessCosts(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCosts, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer) =>
            DiplomacyCostCalculator.DetermineCostForFormingAlliance(kingdom, otherKingdom, forcePlayerCosts, kingdomPartyType).ApplyCost();

        protected override float GetActionScoreInternal(Kingdom kingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType) =>
            AllianceScoringModel.Instance.GetScore(otherKingdom, kingdom, kingdomPartyType).ResultNumber;

        protected override Dictionary<InvolvedParty, List<Kingdom>>? GetInvolvedKingdoms(Kingdom proposingKingdom, Kingdom otherKingdom)
        {
            if (TryGetIvolvees(proposingKingdom, otherKingdom, out var proposingPartyIvolvees) | TryGetIvolvees(otherKingdom, proposingKingdom, out var receivingPartyIvolvees))
            {
                Dictionary<InvolvedParty, List<Kingdom>> involvedKingdoms = new();
                if (proposingPartyIvolvees.Count > 0)
                {
                    involvedKingdoms.Add(InvolvedParty.ProposingParty, proposingPartyIvolvees);
                }
                if (receivingPartyIvolvees.Count > 0)
                {
                    involvedKingdoms.Add(InvolvedParty.ReceivingParty, receivingPartyIvolvees);
                }
                return involvedKingdoms;
            }
            else
            {
                return base.GetInvolvedKingdoms(proposingKingdom, otherKingdom); //null
            }

            bool TryGetIvolvees(Kingdom kingdom, Kingdom otherPartyKingdom, out List<Kingdom> involvedKingdoms)
            {
                involvedKingdoms = Kingdom.All.Where(k => k != kingdom && k != otherPartyKingdom
                                                          && FactionManager.IsAlliedWithFaction(k, kingdom)
                                                          && !FactionManager.IsAlliedWithFaction(k, otherPartyKingdom)).ToList();
                return involvedKingdoms.Any();
            }
        }

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

        protected override void ShowPlayerIvolvedInquiry(Kingdom proposingKingdom, Kingdom otherKingdom, InvolvedParty involvedParty, Action acceptAction)
        {
            _TIvolvedInquiry.SetTextVariable("KINGDOM", proposingKingdom.Name);
            _TIvolvedInquiry.SetTextVariable("OTHER_KINGDOM", otherKingdom.Name);
            _TIvolvedInquiry.SetTextVariable("PLAYER_KINGDOM", Clan.PlayerClan.Kingdom.Name);
            _TIvolvedInquiry.SetTextVariable("NEW_LINE", "\n");

            InformationManager.ShowInquiry(new InquiryData(
                _TAllianceProposal.ToString(),
                _TIvolvedInquiry.ToString(),
                true,
                true,
                new TextObject(StringConstants.Accept).ToString(),
                new TextObject(StringConstants.Decline).ToString(),
                acceptAction,
                null), true);
        }
    }
}
