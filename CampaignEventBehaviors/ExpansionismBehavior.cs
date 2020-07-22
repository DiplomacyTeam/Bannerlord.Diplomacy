using DiplomacyFixes.DiplomaticAction.NonAggressionPact;
using DiplomacyFixes.DiplomaticAction.WarPeace;
using DiplomacyFixes.Extensions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

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
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, UpdateExpansionismDecay);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
        }

        private void DailyTick()
        {
            if (Settings.Instance.EnableCoalitions && MBRandom.RandomFloat < Settings.Instance.CoalitionChancePercentage / 100)
            {
                ConsiderCoalition();
            }
        }

        private void ConsiderCoalition()
        {
        }

        private void UpdateExpansionismDecay(Clan clan)
        {
            if (clan.MapFaction.IsKingdomFaction && clan.Leader == clan.MapFaction.Leader)
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
                if (_expansionismManager == null)
                {
                    this._expansionismManager = new ExpansionismManager();
                }
                this._expansionismManager.Sync();
            }
        }
    }
}
