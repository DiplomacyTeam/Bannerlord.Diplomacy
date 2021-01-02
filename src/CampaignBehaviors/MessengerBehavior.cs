using Diplomacy.Messengers;
using TaleWorlds.CampaignSystem;

namespace Diplomacy.CampaignBehaviors
{
    class MessengerBehavior : CampaignBehaviorBase
    {
        private MessengerManager _messengerManager;

        public MessengerBehavior()
        {
            _messengerManager = new MessengerManager();
        }

        public override void RegisterEvents()
        {
            Events.MessengerSent.AddNonSerializedListener(this, OnMessengerSent);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HasMessengerArrived);
        }

        private void OnMessengerSent(Hero hero)
        {
            _messengerManager.SendMessengerWithCost(hero, DiplomacyCostCalculator.DetermineCostForSendingMessenger());
        }

        public void HasMessengerArrived()
        {
            _messengerManager.MessengerArrived();
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_messengerManager", ref _messengerManager);
            if (dataStore.IsLoading)
            {
                if (_messengerManager is null)
                {
                    _messengerManager = new MessengerManager();
                }
                _messengerManager.Sync();
            }
        }
    }
}
