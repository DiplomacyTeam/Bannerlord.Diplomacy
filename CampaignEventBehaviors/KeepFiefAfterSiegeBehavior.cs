using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    class KeepFiefAfterSiegeBehavior : CampaignBehaviorBase
    {
        private List<SettlementClaimantDecision> _decisionsToProcess;

        public KeepFiefAfterSiegeBehavior()
        {
            this._decisionsToProcess = new List<SettlementClaimantDecision>();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.KingdomDecisionAdded.AddNonSerializedListener(this, AggregateKeepFiefDecisions);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, KeepFief);
        }

        private void KeepFief()
        {
            if (_decisionsToProcess.IsEmpty())
            {
                return;
            }

            RemoveExpiredDecisions();

            SettlementClaimantDecision processedDecision = null;
            foreach (SettlementClaimantDecision decision in _decisionsToProcess)
            {
                processedDecision = decision;

                if (Campaign.Current.KingdomDecisions.Contains(decision))
                {
                    ShowKeepFiefInquiry(decision);
                }

                break;
            }
            if (processedDecision != null)
            {
                _decisionsToProcess.Remove(processedDecision);
            }
        }

        private void RemoveExpiredDecisions()
        {
            IEnumerable<SettlementClaimantDecision> expiredDecisions =
                _decisionsToProcess.Where(decisionToProcess => !Campaign.Current.KingdomDecisions.Contains(decisionToProcess));

            if (expiredDecisions.Any())
            {
                _decisionsToProcess.RemoveAll(decision => expiredDecisions.Contains(decision));
            }
        }

        private void AggregateKeepFiefDecisions(KingdomDecision kingdomDecision, bool isPlayerInvolved)
        {
            SettlementClaimantDecision settlementClaimantDecision = kingdomDecision as SettlementClaimantDecision;
            if (isPlayerInvolved && settlementClaimantDecision != null && settlementClaimantDecision.Settlement.LastAttackerParty.LeaderHero.IsHumanPlayerCharacter)
            {
                _decisionsToProcess.Add(settlementClaimantDecision);
            }
        }

        private void ShowKeepFiefInquiry(SettlementClaimantDecision settlementClaimantDecision)
        {
            InformationManager.ShowInquiry(
                new InquiryData(
                    new TextObject("{=N06wk0dB}Settlement Captured").ToString(),
                    GetKeepFiefText(settlementClaimantDecision).ToString(),
                    true, true,
                    new TextObject("{=Y94H6XnK}Accept").ToString(),
                    new TextObject("{=cOgmdp9e}Decline").ToString(),
                    () =>
                        {
                            ChangeOwnerOfSettlementAction.ApplyByDefault(Hero.MainHero, settlementClaimantDecision.Settlement);
                            Campaign.Current.RemoveDecision(settlementClaimantDecision);
                        },
                    null,
                    ""),
                true);
        }

        private TextObject GetKeepFiefText(SettlementClaimantDecision settlementClaimantDecision)
        {
            TextObject textObject = new TextObject("{=Zy0yjTha}As the capturer of {SETTLEMENT_NAME}, you have the right of first refusal. Would you like to claim this fief?");
            textObject.SetTextVariable("SETTLEMENT_NAME", settlementClaimantDecision.Settlement.Name);

            return textObject;
        }

        public override void SyncData(IDataStore dataStore) { }
    }
}
