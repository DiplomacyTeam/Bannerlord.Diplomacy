using Diplomacy.Character;
using Diplomacy.Events;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class KeepFiefAfterSiegeBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents() => DiplomacyEvents.PlayerSettlementTaken.AddNonSerializedListener(this, OnPlayerSettlementTaken);

        public override void SyncData(IDataStore dataStore) { }

        private void OnPlayerSettlementTaken(Settlement settlement)
        {
            var siegePartyLeader = settlement.LastAttackerParty?.LeaderHero;

            if (siegePartyLeader is null || siegePartyLeader.Clan is null)
                return;

            if (settlement.Town.IsOwnerUnassigned
                && siegePartyLeader.IsHumanPlayerCharacter
                && !siegePartyLeader.Clan.IsUnderMercenaryService)
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
                    new TextObject(StringConstants.Accept).ToString(),
                    new TextObject(StringConstants.Decline).ToString(),
                    () =>
                    {
                        settlement.Town.IsOwnerUnassigned = false;
                        ChangeOwnerOfSettlementAction.ApplyByDefault(Hero.MainHero, settlement);

                        // lose generosity when keeping fief
                        PlayerCharacterTraitEventExperience.FiefClaimed.Apply();
                    },
                    () => settlement.Town.IsOwnerUnassigned = true),
                true);
        }

        private TextObject GetKeepFiefText(Settlement settlement)
        {
            var txt = new TextObject("{=Zy0yjTha}As the capturer of {SETTLEMENT_NAME}, you have the right of first refusal. Would you like to claim this fief?");
            txt.SetTextVariable("SETTLEMENT_NAME", settlement.Name);
            return txt;
        }
    }
}