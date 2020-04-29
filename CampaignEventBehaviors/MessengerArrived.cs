using DiplomacyFixes.Messengers;
using System;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    class MessengerArrived : CampaignBehaviorBase
    {

        public override void RegisterEvents()
        {
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(this.HasMessengerArrived));
        }

        public void HasMessengerArrived()
        {
            foreach (Messenger messenger in MessengerManager.Instance.Messengers)
            {
                if (messenger.DispatchTime.ElapsedDaysUntilNow > Settings.Instance.MessengerTravelTime)
                {
                    MessengerManager.Instance.MessengerArrived(messenger);
                    break;
                }
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
