using Diplomacy.Messengers;

using TaleWorlds.CampaignSystem;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class MessengerBehavior : CampaignBehaviorBase
    {
        private MessengerManager _messengerManager;

        public MessengerBehavior() => _messengerManager = new();

        public override void RegisterEvents()
        {
            Events.MessengerSent.AddNonSerializedListener(this, OnMessengerSent);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HasMessengerArrivedHourlyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_messengerManager", ref _messengerManager);

            if (dataStore.IsLoading)
            {
                _messengerManager ??= new();
                _messengerManager.Sync();
            }
        }

        private void OnMessengerSent(Hero hero)
            => _messengerManager.SendMessengerWithCost(hero, DiplomacyCostCalculator.DetermineCostForSendingMessenger());

        public void HasMessengerArrivedHourlyTick() => _messengerManager.MessengerArrived();
    }
}
