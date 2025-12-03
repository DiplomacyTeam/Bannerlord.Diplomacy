using Diplomacy.Events;

using Microsoft.Extensions.Logging;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class CooldownBehavior : CampaignBehaviorBase
    {
        private CooldownManager _cooldownManager;

        public CooldownBehavior()
        {
            _cooldownManager = new();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.MakePeace.AddNonSerializedListener(this, RegisterDeclareWarCooldown);
            DiplomacyEvents.PeaceProposalSent.AddNonSerializedListener(this, RegisterPeaceProposalCooldown);
            CampaignEvents.OnAllianceStartedEvent.AddNonSerializedListener(this, RegisterAllianceFormedCooldown);
        }

        private void RegisterAllianceFormedCooldown(Kingdom kingdom1, Kingdom kingdom2)
        {
            LogFactory.Get<CooldownBehavior>()
                .LogTrace($"[{CampaignTime.Now}] {kingdom1} got an alliance formation cooldown with {kingdom2}.");

            _cooldownManager.UpdateLastAllianceFormedTime(kingdom1, kingdom2, CampaignTime.Now);
        }

        private void RegisterDeclareWarCooldown(IFaction faction1, IFaction faction2, MakePeaceAction.MakePeaceDetail makePeaceDetail)
        {
            if (faction1 is Kingdom kingdom1 && faction2 is Kingdom kingdom2)
            {
                LogFactory.Get<CooldownBehavior>().LogTrace($"[{CampaignTime.Now}] {kingdom1.Name} got a war declaration cooldown with {kingdom2.Name}.");
            }
        }

        private void RegisterPeaceProposalCooldown(Kingdom kingdom)
        {
            LogFactory.Get<CooldownBehavior>()
                .LogTrace($"[{CampaignTime.Now}] {kingdom.Name} sent a peace proposal.");

            _cooldownManager.UpdateLastPeaceProposalTime(kingdom, CampaignTime.Now);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_cooldownManager", ref _cooldownManager);

            if (dataStore.IsLoading)
            {
                _cooldownManager ??= new();
                _cooldownManager.Sync();
            }
        }
    }
}