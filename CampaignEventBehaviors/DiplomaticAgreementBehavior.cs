using DiplomacyFixes.DiplomaticAction;
using DiplomacyFixes.DiplomaticAction.Alliance;
using System;
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
            Events.AllianceFormed.AddNonSerializedListener(this, this.ExpireNonAggressionPact);
        }

        private void ExpireNonAggressionPact(AllianceEvent obj)
        {
            if(DiplomaticAgreementManager.Instance.HasNonAggressionPact(obj.Kingdom, obj.OtherKingdom, out NonAggressionPactAgreement pactAgreement))
            {
                pactAgreement.Expire();
            }
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
