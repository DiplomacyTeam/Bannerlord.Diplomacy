using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    class ExpansionismBehavior : CampaignBehaviorBase
    {
        private ExpansionismManager _expansionismManager;

        public ExpansionismBehavior()
        {
            this._expansionismManager = new ExpansionismManager();
        }
        public override void RegisterEvents()
        {
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, UpdateExpasionismScore);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, UpdateExpansionDecay);
        }

        private void UpdateExpansionDecay(Clan clan)
        {
            if(clan.MapFaction.IsKingdomFaction && clan.Leader == clan.MapFaction.Leader)
            {
                this._expansionismManager.ApplyExpansionismDecay(clan.MapFaction);
            }
        }

        private void UpdateExpasionismScore(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (newOwner.MapFaction != oldOwner.MapFaction && newOwner.MapFaction.IsKingdomFaction && detail == ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.BySiege)
            {
                this._expansionismManager.AddSiegeScore(newOwner.MapFaction);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_expansionismManager", ref _expansionismManager);
            if (dataStore.IsLoading)
            {
                this._expansionismManager.Sync();
            }
        }
    }
}
