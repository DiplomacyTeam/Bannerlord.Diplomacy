using Bannerlord.BUTR.Shared.Extensions;

using Diplomacy.DiplomaticAction.Conditioning;
using Diplomacy.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;

namespace Diplomacy.DiplomaticAction
{
    abstract class AbstractDiplomaticAction<T> where T : AbstractDiplomaticAction<T>, new()
    {
        protected static T Instance { get; } = new();
        protected virtual bool JointResolutionRequired => false;
        protected virtual float JointResolutionThreshold => 0f;
        protected virtual Dictionary<(Kingdom EvaluatingKingdom, Kingdom OtherKingdom), (CampaignTime ScoreTimestamp, float ScoreValue)> ActionScoreCache { get; } = new();

        public static void Apply(Kingdom proposingKingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, bool bypassCosts = false, float? customDurationInDays = null, bool queryPlayer = true)

        {
            Instance.TryApply(proposingKingdom, otherKingdom, forcePlayerCharacterCosts, bypassCosts, customDurationInDays, queryPlayer);
        }

        public static bool CanApply(Kingdom proposingKingdom, Kingdom otherKingdom, DiplomacyConditionType accountedConditions = DiplomacyConditionType.All, bool forcePlayerCharacterCosts = false)
        {
            if (Instance.JointResolutionRequired && accountedConditions.HasFlag(DiplomacyConditionType.ScoreRelated))
            {
                //It is assumed that any ScoreCondition for the JointResolution action will check against auto-rejection threshold and not acceptance threshold!
                var involvedKingdoms = Instance.GetInvolvedKingdoms(proposingKingdom, otherKingdom);
                return
                    Instance.AllInvolvedKingdomsPassConditions(proposingKingdom, otherKingdom, involvedKingdoms, accountedConditions, forcePlayerCharacterCosts)
                    && Instance.JointResolutionReached(proposingKingdom, otherKingdom, involvedKingdoms, forcePlayerCharacterCosts);
            }
            else
            {
                return
                    Instance.AllInvolvedKingdomsPassConditions(proposingKingdom, otherKingdom, Instance.GetInvolvedKingdoms(proposingKingdom, otherKingdom), accountedConditions, forcePlayerCharacterCosts);
            }
        }

        public static float GetActionScore(Kingdom kingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType)
        {
            if (Instance.ActionScoreCache.TryGetValue((kingdom, otherKingdom), out var scoreInfo) && scoreInfo.ScoreTimestamp == CampaignTime.Now)
            {
                return scoreInfo.ScoreValue;
            }

            float scoreValue = Instance.GetActionScoreInternal(kingdom, otherKingdom, kingdomPartyType);
            Instance.ActionScoreCache[(kingdom, otherKingdom)] = (CampaignTime.Now, scoreValue);
            return scoreValue;
        }

        protected virtual bool AllInvolvedKingdomsPassConditions(Kingdom proposingKingdom, Kingdom otherKingdom,
                                                                 Dictionary<InvolvedParty, List<Kingdom>>? involvedKingdoms,
                                                                 DiplomacyConditionType accountedConditions = DiplomacyConditionType.All,
                                                                 bool forcePlayerCharacterCosts = false)
        {
            GetFullPartyLists(proposingKingdom, otherKingdom, involvedKingdoms, out var proposingPartyKingdoms, out var receivingPartyKingdoms);
            return involvedKingdoms is null || !(FailcheckPartyInvolvees(proposingPartyKingdoms, receivingPartyKingdoms) || FailcheckPartyInvolvees(receivingPartyKingdoms, proposingPartyKingdoms));

            //Local functions
            bool FailcheckPartyInvolvees(Dictionary<Kingdom, DiplomaticPartyType> partyKingdoms, Dictionary<Kingdom, DiplomaticPartyType> otherPartyKingdoms) =>
                partyKingdoms.SelectMany(proposer => otherPartyKingdoms.Select(receiver => (proposer, receiver)))
                             .Any(pair => !Instance.PassesConditions(pair.proposer.Key, pair.receiver.Key, accountedConditions, forcePlayerCharacterCosts, pair.proposer.Value));
        }

        protected virtual bool JointResolutionReached(Kingdom proposingKingdom, Kingdom otherKingdom, Dictionary<InvolvedParty, List<Kingdom>>? involvedKingdoms, bool forcePlayerCharacterCosts = false)
        {
            GetFullPartyLists(proposingKingdom, otherKingdom, involvedKingdoms, out var proposingPartyKingdoms, out var receivingPartyKingdoms);
            return
                GetPartyCheckSum(proposingPartyKingdoms, receivingPartyKingdoms) >= Instance.JointResolutionThreshold
                && GetPartyCheckSum(receivingPartyKingdoms, proposingPartyKingdoms) >= Instance.JointResolutionThreshold;

            float GetPartyCheckSum(Dictionary<Kingdom, DiplomaticPartyType> partyKingdoms, Dictionary<Kingdom, DiplomaticPartyType> otherPartyKingdoms) =>
                partyKingdoms.SelectMany(proposer => otherPartyKingdoms.Select(receiver => (proposer, receiver)))
                             .Sum(pair => (GetActionScore(pair.proposer.Key, pair.receiver.Key, pair.proposer.Value) - GetActionThreshold()) * Instance.GetKingdomWeight(pair.proposer.Key));
        }

        protected virtual float GetKingdomWeight(Kingdom kingdom) => 1f;

        protected static void GetFullPartyLists(Kingdom proposingKingdom, Kingdom otherKingdom,
                                                Dictionary<InvolvedParty, List<Kingdom>>? involvedKingdoms,
                                                out Dictionary<Kingdom, DiplomaticPartyType> proposingPartyKingdoms,
                                                out Dictionary<Kingdom, DiplomaticPartyType> receivingPartyKingdoms)
        {
            proposingPartyKingdoms = new() { { proposingKingdom, DiplomaticPartyType.Proposer } };
            receivingPartyKingdoms = new() { { otherKingdom, DiplomaticPartyType.Recipient } };
            if (involvedKingdoms is not null)
            {
                if (involvedKingdoms.TryGetValue(InvolvedParty.ProposingParty, out var proposingPartyList))
                {                    
                    proposingPartyKingdoms.AddRange(proposingPartyList.ToDictionary(key => key, value => DiplomaticPartyType.ProposerInvolvee));
                }
                if (involvedKingdoms.TryGetValue(InvolvedParty.ReceivingParty, out var receivingPartyList))
                {
                    receivingPartyKingdoms.AddRange(receivingPartyList.ToDictionary(key => key, value => DiplomaticPartyType.ReceiverInvolvee));
                }
            }
        }

        protected void TryApply(Kingdom proposingKingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts, bool bypassCosts, float? customDurationInDays, bool queryPlayer)
        {
            bool playerWasQueried = false;
            var involvedKingdoms = Instance.GetInvolvedKingdoms(proposingKingdom, otherKingdom);
            GetFullPartyLists(proposingKingdom, otherKingdom, involvedKingdoms, out var proposingPartyKingdoms, out var receivingPartyKingdoms);

            if (queryPlayer)
            {
                if (otherKingdom.IsRuledByPlayer())
                {
                    Instance.ShowPlayerInquiry(proposingKingdom, applyAction);
                    playerWasQueried = true;
                }
                else if (involvedKingdoms is not null)
                {
                    playerWasQueried = NotifyIfPlayerInvolved(involvedKingdoms, InvolvedParty.ProposingParty, applyAction) || NotifyIfPlayerInvolved(involvedKingdoms, InvolvedParty.ReceivingParty, applyAction);
                }
            }

            if (!playerWasQueried)
                Instance.ApplyInternalWithCosts(proposingPartyKingdoms, receivingPartyKingdoms, forcePlayerCharacterCosts, bypassCosts, customDurationInDays);

            //Local functions
            void applyAction() => Instance.ApplyInternalWithCosts(proposingPartyKingdoms, receivingPartyKingdoms, forcePlayerCharacterCosts, bypassCosts, customDurationInDays);

            bool CheckPartyInvolvees(Dictionary<InvolvedParty, List<Kingdom>> involvedKingdoms, InvolvedParty involvedParty) =>
                involvedKingdoms.TryGetValue(involvedParty, out var partyKngdomList) && partyKngdomList.Any(involvedKingdom => involvedKingdom.IsRuledByPlayer());

            bool NotifyIfPlayerInvolved(Dictionary<InvolvedParty, List<Kingdom>> involvedKingdoms, InvolvedParty involvedParty, Action applyAction)
            {
                bool playerWasQueried = false;
                if (CheckPartyInvolvees(involvedKingdoms, involvedParty))
                {
                    Instance.ShowPlayerIvolvedInquiry(proposingKingdom, otherKingdom, involvedParty, applyAction);
                    playerWasQueried = true;
                }
                return playerWasQueried;
            }
        }

        protected void ApplyInternalWithCosts(Dictionary<Kingdom, DiplomaticPartyType> proposingPartyKingdoms, Dictionary<Kingdom, DiplomaticPartyType> receivingPartyKingdoms, bool forcePlayerCharacterCosts, bool bypassCosts, float? customDurationInDays)
        {
            if (!bypassCosts)
            {
                ApplyPartyCosts(proposingPartyKingdoms, receivingPartyKingdoms);
                ApplyPartyCosts(receivingPartyKingdoms, proposingPartyKingdoms);

            }
            DoApply(proposingPartyKingdoms, receivingPartyKingdoms);

            //Local functions
            void ApplyPartyCosts(Dictionary<Kingdom, DiplomaticPartyType> partyKingdoms, Dictionary<Kingdom, DiplomaticPartyType> otherPartyKingdoms) =>
                partyKingdoms.SelectMany(proposer => otherPartyKingdoms.Select(receiver => (proposer, receiver))).ToList()
                             .ForEach(pair => Instance.AssessCosts(pair.proposer.Key, pair.receiver.Key, forcePlayerCharacterCosts, pair.proposer.Value));

            void DoApply(Dictionary<Kingdom, DiplomaticPartyType> partyKingdoms, Dictionary<Kingdom, DiplomaticPartyType> otherPartyKingdoms) =>
                partyKingdoms.SelectMany(proposer => otherPartyKingdoms.Select(receiver => (proposer, receiver))).ToList()
                             .ForEach(pair => Instance.ApplyInternal(pair.proposer.Key, pair.receiver.Key, customDurationInDays));
        }

        public abstract bool PassesConditions(Kingdom kingdom, Kingdom otherKingdom, DiplomacyConditionType accountedConditions = DiplomacyConditionType.All, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer);

        protected abstract float GetActionScoreInternal(Kingdom kingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType);

        protected abstract float GetActionThreshold();

        protected abstract void ShowPlayerInquiry(Kingdom proposingKingdom, Action acceptAction);

        protected abstract void ShowPlayerIvolvedInquiry(Kingdom proposingKingdom, Kingdom otherKingdom, InvolvedParty involvedParty, Action acceptAction);

        protected abstract void ApplyInternal(Kingdom kingdom, Kingdom otherKingdom, float? customDurationInDays);

        protected abstract void AssessCosts(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer);

        protected virtual Dictionary<InvolvedParty, List<Kingdom>>? GetInvolvedKingdoms(Kingdom proposingKingdom, Kingdom otherKingdom) => null;
    }

    internal enum InvolvedParty : byte
    {
        ProposingParty = 0,
        ReceivingParty = 1
    }
}