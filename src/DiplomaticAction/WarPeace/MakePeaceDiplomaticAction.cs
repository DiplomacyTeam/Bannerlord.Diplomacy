using Diplomacy.Costs;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Barterables;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace
{
    //FIXME: Need to carefully integrate this action with the StoryMode campaign. Not sure if SatisfiesQuestConditionsForPeaceCondition will be enough.
    internal sealed class MakePeaceDiplomaticAction : AbstractDiplomaticAction<MakePeaceDiplomaticAction> //Added "Diplomatic" to avoid confusion with the TaleWorlds.CampaignSystem.Actions.MakePeaceAction.
    {
        private static readonly TextObject _TInquiry = new("{=t0ZS9maD}{KINGDOM_LEADER} of the {KINGDOM} is proposing peace with the {PLAYER_KINGDOM}.");
        private static readonly TextObject _TIvolvedInquiry = new("{=rq0SHLq2}Kingdom {KINGDOM}{?INVOLVED_WITH_PROPOSER}, allied to kingdom {PLAYER_KINGDOM}, proposes peace treaty with {OTHER_KINGDOM}{?} proposes peace treaty with {OTHER_KINGDOM}, which is an ally of {PLAYER_KINGDOM}{//?}.{NEW_LINE} {NEW_LINE}As an active member of the existing alliance, you must vote whether or not to accept the peace. The treaty will be approved only if all the parties involved agree with it.");
        private static readonly TextObject _TPeaceProposal = new("{=BkGSVccZ}Peace Proposal");

        protected override bool JointResolutionRequired => true;

        private Dictionary<(Kingdom EvaluatingKingdom, Kingdom OtherKingdom), (CampaignTime EvaluationTimestamp, int TributeValue)> TributeCache { get; } = new();

        private static int GetTribute(Kingdom kingdom, Kingdom otherKingdom)
        {
            if (Instance.TributeCache.TryGetValue((kingdom, otherKingdom), out var tributeInfo) && tributeInfo.EvaluationTimestamp == CampaignTime.Now)
            {
                return tributeInfo.TributeValue;
            }

            int tributeValue = Instance.GetTributeInternal(kingdom, otherKingdom);
            Instance.ActionScoreCache[(kingdom, otherKingdom)] = (CampaignTime.Now, tributeValue);
            return tributeValue;
        }

        public static void ApplyPeace(Kingdom kingdom, Kingdom otherKingdom)
        {
            if (kingdom.IsAtWarWith(otherKingdom))
            {
                LogFactory.Get<MakePeaceDiplomaticAction>().LogTrace($"[{CampaignTime.Now}] {kingdom.Name} secured peace with {otherKingdom.Name}.");
                MakePeaceAction.Apply(kingdom, otherKingdom, GetTribute(kingdom, otherKingdom));
            }
        }

        public override bool PassesConditions(Kingdom kingdom, Kingdom otherKingdom, DiplomacyConditionType accountedConditions = DiplomacyConditionType.All, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer) =>
            MakePeaceConditions.Instance.CanApply(kingdom, otherKingdom, accountedConditions, forcePlayerCharacterCosts, kingdomPartyType);

        protected override void ApplyInternal(Kingdom kingdom, Kingdom otherKingdom, float? customDurationInDays)
        {
            ApplyPeace(kingdom, otherKingdom);
        }

        protected override float GetActionScoreInternal(Kingdom kingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType)
        {
            //just in case, we always get tributes from the perspective of the proposing party, as it's done in the game
            int dailyTribute = (kingdomPartyType == DiplomaticPartyType.Proposer || kingdomPartyType == DiplomaticPartyType.ProposerInvolvee) ? GetTribute(kingdom, otherKingdom) : -GetTribute(otherKingdom, kingdom);
            var PeaceDecision = new MakePeaceKingdomDecision(kingdom.RulingClan, otherKingdom, dailyTribute, false);
            var decisionSupport = PeaceDecision.CalculateSupport(kingdom.RulingClan);
            return decisionSupport * Math.Max(GetKingdomSupportForDecision(PeaceDecision, decisionSupport >= 0 ? 0 : 1), 0.1f);

            //Local functions
            static float GetKingdomSupportForDecision(KingdomDecision decision, int outcomeNo) => new KingdomElection(decision).GetLikelihoodForOutcome(outcomeNo);
        }

        protected override void AssessCosts(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer) =>
            DiplomacyCostCalculator.DetermineInfluenceCostForMakingPeace(kingdom, forcePlayerCharacterCosts).ApplyCost();

        protected override void ShowPlayerInquiry(Kingdom proposingKingdom, Action acceptAction)
        {
            _TInquiry.SetTextVariable("KINGDOM_LEADER", proposingKingdom.Leader.Name);
            _TInquiry.SetTextVariable("KINGDOM", proposingKingdom.Name);
            _TInquiry.SetTextVariable("PLAYER_KINGDOM", Clan.PlayerClan.Kingdom.Name);

            InformationManager.ShowInquiry(new InquiryData(
                _TPeaceProposal.ToString(),
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
                _TPeaceProposal.ToString(),
                _TIvolvedInquiry.ToString(),
                true,
                true,
                new TextObject(StringConstants.Accept).ToString(),
                new TextObject(StringConstants.Decline).ToString(),
                acceptAction,
                null), true);
        }

        protected override Dictionary<InvolvedParty, List<Kingdom>>? GetInvolvedKingdoms(Kingdom proposingKingdom, Kingdom otherKingdom)
        {
            //FIXME: extract to a static method somewhere? Used exact same way for the DeclareAllianceAction.
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

        private int GetTributeInternal(Kingdom kingdom, Kingdom otherKingdom)
        {
            PeaceBarterable peaceBarterable = new(kingdom.Leader, kingdom, otherKingdom, CampaignTime.Years(1f));
            int valueForOtherKingdom = -peaceBarterable.GetValueForFaction(otherKingdom);

            foreach (Clan clan in otherKingdom.Clans)
            {
                if (clan.Leader != clan.MapFaction.Leader)
                {
                    int valueForClan = -peaceBarterable.GetValueForFaction(clan);
                    if (valueForClan > valueForOtherKingdom)
                    {
                        valueForOtherKingdom = valueForClan;
                    }
                }
            }
            valueForOtherKingdom = (otherKingdom == Clan.PlayerClan.Kingdom) ? valueForOtherKingdom + 15000 : valueForOtherKingdom; //KingdomDecisionProposalBehavior.ConsiderPeace modifier
            valueForOtherKingdom = (valueForOtherKingdom is > (-5000) and < 5000) ? 0 : valueForOtherKingdom;

            return 10 * (Campaign.Current.Models.DiplomacyModel.GetDailyTributeForValue(valueForOtherKingdom) / 10);
        }
    }
}
