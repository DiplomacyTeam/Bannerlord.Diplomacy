using DiplomacyFixes.Messengers;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    class MessengerBehavior : CampaignBehaviorBase
    {
        private MessengerManager _messengerManager;

        public MessengerBehavior()
        {
            this._messengerManager = new MessengerManager();
        }

        public override void RegisterEvents()
        {
            Events.MessengerSent.AddNonSerializedListener(this, OnMessengerSent);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HasMessengerArrived);
        }

        private void OnMessengerSent(Hero hero)
        {
            _messengerManager.SendMessengerWithInfluenceCost(hero, DiplomacyCostCalculator.DetermineInfluenceCostForSendingMessenger());
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
                if (_messengerManager == null)
                {
                    this._messengerManager = new MessengerManager();
                }
                this._messengerManager.Sync();
            }
        }
    }
}
