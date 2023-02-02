using Diplomacy.Costs;
using Diplomacy.Events;
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
            DiplomacyEvents.MessengerSent.AddNonSerializedListener(this, OnMessengerSent);
            CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener(this, OnAfterSessionLaunched);
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

        public void OnAfterSessionLaunched(CampaignGameStarter game)
        {
            CampaignEvents.TickEvent.AddNonSerializedListener(_messengerManager, _messengerManager.OnAfterSaveLoad);
        }
    }
}