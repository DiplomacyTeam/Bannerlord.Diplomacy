using DiplomacyFixes.DiplomaticAction;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    class DiplomaticAgreementBehavior : CampaignBehaviorBase
    {

        private DiplomaticAgreementManager _diplomaticAgreementManager;

        public DiplomaticAgreementBehavior()
        {
            _diplomaticAgreementManager = new DiplomaticAgreementManager();
        }

        public override void RegisterEvents()
        {
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_diplomaticAgreementManager", ref _diplomaticAgreementManager);
            if (dataStore.IsLoading)
            {
                if (_diplomaticAgreementManager == null)
                {
                    this._diplomaticAgreementManager = new DiplomaticAgreementManager();
                }
                this._diplomaticAgreementManager.Sync();
            }
        }
    }
}
