using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    class KeepFiefAfterSiegeBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            Events.PlayerSettlementTaken.AddNonSerializedListener(this, KeepFief);
        }

        private void KeepFief(Settlement settlement)
        {
            SettlementClaimantDecision settlementClaimantDecision =
                Campaign.Current.KingdomDecisions.OfType<SettlementClaimantDecision>()?.Where(decision => decision.Settlement == settlement).FirstOrDefault();
            if (settlementClaimantDecision != null)
            {
                Hero capturerHero = (Hero)typeof(SettlementClaimantDecision).GetField("_capturerHero", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(settlementClaimantDecision);
                if (capturerHero.IsHumanPlayerCharacter)
                {
                    ShowKeepFiefInquiry(settlement, settlementClaimantDecision);
                }
            }
        }

        private void ShowKeepFiefInquiry(Settlement settlement, SettlementClaimantDecision settlementClaimantDecision)
        {
            InformationManager.ShowInquiry(
                new InquiryData(
                    new TextObject("{=N06wk0dB}Settlement Captured").ToString(),
                    GetKeepFiefText(settlement).ToString(),
                    true, true,
                    new TextObject("{=Y94H6XnK}Accept").ToString(),
                    new TextObject("{=cOgmdp9e}Decline").ToString(),
                    () =>
                        {
                            ChangeOwnerOfSettlementAction.ApplyByDefault(Hero.MainHero, settlement);
                            Campaign.Current.RemoveDecision(settlementClaimantDecision);
                        },
                    null,
                    ""),
                true);
        }

        private TextObject GetKeepFiefText(Settlement settlement)
        {
            TextObject textObject = new TextObject("{=Zy0yjTha}As the capturer of {SETTLEMENT_NAME}, you have the right of first refusal. Would you like to claim this fief?");
            textObject.SetTextVariable("SETTLEMENT_NAME", settlement.Name);

            return textObject;
        }

        public override void SyncData(IDataStore dataStore) { }
    }
}
