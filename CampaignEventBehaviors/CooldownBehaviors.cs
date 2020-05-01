using System;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    class CooldownBehaviors : CampaignBehaviorBase
    {
        private CooldownManager _cooldownManager;

        public CooldownBehaviors()
        {
            this._cooldownManager = new CooldownManager();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.MakePeace.AddNonSerializedListener(this, this.RegisterDeclareWarCooldown);
            Events.PeaceProposalSent.AddNonSerializedListener(this, RegisterPeaceProposalCooldown);
        }

        private void RegisterDeclareWarCooldown(IFaction faction1, IFaction faction2)
        {
            _cooldownManager.UpdateLastWarTime(faction1, faction2, CampaignTime.Now);
        }

        private void RegisterPeaceProposalCooldown(Kingdom kingdom)
        {
            _cooldownManager.UpdateLastPeaceProposalTime(kingdom, CampaignTime.Now);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_cooldownManager", ref _cooldownManager);
            _cooldownManager.sync();
        }
    }
}
