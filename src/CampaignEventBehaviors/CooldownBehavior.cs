using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.DiplomaticAction.NonAggressionPact;
using TaleWorlds.CampaignSystem;

namespace Diplomacy.CampaignEventBehaviors
{
    class CooldownBehavior : CampaignBehaviorBase
    {
        private CooldownManager _cooldownManager;

        public CooldownBehavior()
        {
            this._cooldownManager = new CooldownManager();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.MakePeace.AddNonSerializedListener(this, RegisterDeclareWarCooldown);
            Events.PeaceProposalSent.AddNonSerializedListener(this, RegisterPeaceProposalCooldown);
            Events.AllianceFormed.AddNonSerializedListener(this, RegisterAllianceFormedCooldown);
        }

        private void RegisterAllianceFormedCooldown(AllianceEvent allianceFormedEvent)
        {
            _cooldownManager.UpdateLastAllianceFormedTime(allianceFormedEvent.Kingdom, allianceFormedEvent.OtherKingdom, CampaignTime.Now);
        }

        private void RegisterDeclareWarCooldown(IFaction faction1, IFaction faction2)
        {
            Kingdom kingdom1, kingdom2;
            kingdom1 = faction1 as Kingdom;
            kingdom2 = faction2 as Kingdom;

            if (kingdom1 != null && kingdom2 != null)
            {
                FormNonAggressionPactAction.Apply(kingdom1, kingdom2, bypassCosts: true, customDurationInDays: Settings.Instance.DeclareWarCooldownInDays, queryPlayer: false);
            }
        }

        private void RegisterPeaceProposalCooldown(Kingdom kingdom)
        {
            _cooldownManager.UpdateLastPeaceProposalTime(kingdom, CampaignTime.Now);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_cooldownManager", ref _cooldownManager);
            if (dataStore.IsLoading)
            {
                if (_cooldownManager == null)
                {
                    _cooldownManager = new CooldownManager();
                }
                _cooldownManager.Sync();
            }
        }
    }
}
