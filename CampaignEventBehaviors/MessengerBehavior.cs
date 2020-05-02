using DiplomacyFixes.Messengers;
using System;
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
            foreach (Messenger messenger in _messengerManager.Messengers)
            {
                if (messenger.DispatchTime.ElapsedDaysUntilNow > Settings.Instance.MessengerTravelTime)
                {
                    _messengerManager.MessengerArrived(messenger);
                    break;
                }
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_messengerManager", ref _messengerManager);
            _messengerManager.Sync();
        }
    }
}
