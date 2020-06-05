using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
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
                if (settlement.Town.IsOwnerUnassigned && (settlement.LastAttackerParty?.LeaderHero?.IsHumanPlayerCharacter ?? false))
                {
                    ShowKeepFiefInquiry(settlement);
                }
        }

        private void ShowKeepFiefInquiry(Settlement settlement)
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
                            settlement.Town.IsOwnerUnassigned = false;
                            ChangeOwnerOfSettlementAction.ApplyByDefault(Hero.MainHero, settlement);
                        },
                    () =>
                        {
                            settlement.Town.IsOwnerUnassigned = true;
                        },
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
