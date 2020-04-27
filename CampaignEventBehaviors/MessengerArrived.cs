using DiplomacyFixes.Messengers;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    class MessengerArrived : CampaignBehaviorBase
    {
        [SaveableField(1)]
        private List<Messenger> _messengers;
        private MessengerManager _messengerManager;

        public MessengerArrived()
        {
            this._messengers = new List<Messenger>();
            this._messengerManager = new MessengerManager(_messengers);
        }

        public override void RegisterEvents()
        {
            Events.MessengerSent.AddNonSerializedListener(this, new Action<Hero>(this.OnMessengerSent));
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(this.HasMessengerArrived));
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
            dataStore.SyncData("_messengers", ref _messengers);
            _messengerManager = new MessengerManager(_messengers);
        }
    }
}
