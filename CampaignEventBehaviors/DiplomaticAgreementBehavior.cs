using DiplomacyFixes.DiplomaticAction;
using DiplomacyFixes.DiplomaticAction.Alliance;
using DiplomacyFixes.DiplomaticAction.NonAggressionPact;
using DiplomacyFixes.Extensions;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

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
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, this.UpdateDiplomaticAgreements);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, this.ConsiderDiplomaticAgreements);
            Events.AllianceFormed.AddNonSerializedListener(this, this.ExpireNonAggressionPact);
        }

        private void ConsiderDiplomaticAgreements(Clan clan)
        {
            // only apply to kingdom leader clans
            if (clan.MapFaction.IsKingdomFaction && clan.MapFaction.Leader == clan.Leader && !clan.Leader.IsHumanPlayerCharacter)
            {
                this.ConsiderNonAggressionPact(clan.MapFaction as Kingdom);
            }
        }

        private void ConsiderNonAggressionPact(Kingdom proposingKingdom)
        {
            if (MBRandom.RandomFloat < 0.10f)
            {
                Kingdom proposedKingdom = Kingdom.All.Where(kingdom => kingdom != proposingKingdom)?
                    .Where(kingdom => NonAggressionPactConditions.Instance.CanExecuteAction(proposingKingdom, kingdom))
                    .OrderByDescending(kingdom => kingdom.GetExpansionism()).FirstOrDefault();

                if (proposedKingdom != null) {
                    FormNonAggressionPactAction.Apply(proposingKingdom, proposedKingdom);
                }
            }
        }

        private void UpdateDiplomaticAgreements()
        {
            DiplomaticAgreementManager.Instance.Agreements.Values.SelectMany(x => x).ToList().ForEach(x => x.TryExpireNotification());
        }

        private void ExpireNonAggressionPact(AllianceEvent obj)
        {
            if (DiplomaticAgreementManager.Instance.HasNonAggressionPact(obj.Kingdom, obj.OtherKingdom, out NonAggressionPactAgreement pactAgreement))
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
