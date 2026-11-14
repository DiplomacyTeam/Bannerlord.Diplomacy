using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class FiefProvenanceBehavior : CampaignBehaviorBase
    {
        private FiefProvenanceManager _fiefProvenanceManager;

        public FiefProvenanceBehavior() => _fiefProvenanceManager = new();

        public override void RegisterEvents()
        {
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, (clan, oldKingdom, newKingdom, _, _) => _fiefProvenanceManager.RegisterKingdomChange(clan, oldKingdom, newKingdom));
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_fiefProvenanceManager", ref _fiefProvenanceManager);

            if (dataStore.IsLoading)
            {
                _fiefProvenanceManager ??= new();
                _fiefProvenanceManager.Sync();
            }
        }

        private void OnSettlementOwnerChanged(Settlement settlement,
                                              bool openToClaim,
                                              Hero newOwner,
                                              Hero oldOwner,
                                              Hero capturerHero,
                                              ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            // A clan leaving its kingdom is taken out of it before its fiefs pass to the ruler, so by now the old
            // owner has no kingdom. The fief never left the ruler's kingdom; recording it as coming from no kingdom
            // would erase which kingdom it was conquered from.
            var leftKingdom = detail == ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByLeaveFaction && oldOwner?.Clan?.Kingdom is null;

            _fiefProvenanceManager.RegisterTransfer(settlement,
                                                    oldOwner,
                                                    newOwner,
                                                    detail == ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.BySiege,
                                                    leftKingdom ? newOwner?.MapFaction as Kingdom : null);
        }
    }
}
