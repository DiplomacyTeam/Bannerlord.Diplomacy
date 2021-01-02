using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Diplomacy.CampaignBehaviors
{
    class ExpansionismBehavior : CampaignBehaviorBase
    {
        private ExpansionismManager _expansionismManager;

        public ExpansionismBehavior()
        {
            _expansionismManager = new();
        }
        public override void RegisterEvents()
        {
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, UpdateExpasionismScore);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, UpdateExpansionismDecay);
        }

        private void UpdateExpansionismDecay(Clan clan)
        {
            if (clan.MapFaction.IsKingdomFaction && clan.Leader == clan.MapFaction.Leader)
            {
                _expansionismManager.ApplyExpansionismDecay(clan.MapFaction);
            }
        }

        private void UpdateExpasionismScore(Settlement settlement,
                                            bool openToClaim,
                                            Hero newOwner,
                                            Hero oldOwner,
                                            Hero capturerHero,
                                            ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (newOwner.MapFaction != oldOwner.MapFaction
                && newOwner.MapFaction.IsKingdomFaction
                && detail == ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.BySiege)
            {
                _expansionismManager.AddSiegeScore(newOwner.MapFaction);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_expansionismManager", ref _expansionismManager);
            if (dataStore.IsLoading)
            {
                _expansionismManager ??= new();
                _expansionismManager.Sync();
            }
        }
    }
}
