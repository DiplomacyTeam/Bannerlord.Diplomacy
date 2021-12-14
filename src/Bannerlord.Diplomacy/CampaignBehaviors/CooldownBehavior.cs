using Diplomacy.CivilWar;
using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.DiplomaticAction.NonAggressionPact;
using Diplomacy.Event;
using Diplomacy.Extensions;

using Microsoft.Extensions.Logging;

using TaleWorlds.CampaignSystem;

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
            Events.PeaceProposalSent.AddNonSerializedListener(this, RegisterPeaceProposalCooldown);
            Events.AllianceFormed.AddNonSerializedListener(this, RegisterAllianceFormedCooldown);
        }

        private void RegisterAllianceFormedCooldown(AllianceEvent allianceFormedEvent)
        {
            LogFactory.Get<CooldownBehavior>()
                .LogTrace($"[{CampaignTime.Now}] {allianceFormedEvent.Kingdom} got an alliance formation cooldown with {allianceFormedEvent.OtherKingdom}.");

            _cooldownManager.UpdateLastAllianceFormedTime(allianceFormedEvent.Kingdom, allianceFormedEvent.OtherKingdom, CampaignTime.Now);
        }

        private void RegisterDeclareWarCooldown(IFaction faction1, IFaction faction2)
        {
            if (faction1 is Kingdom kingdom1 && faction2 is Kingdom kingdom2)
            {
                LogFactory.Get<CooldownBehavior>()
                    .LogTrace($"[{CampaignTime.Now}] {kingdom1.Name} got a war declaration cooldown with {kingdom2.Name}.");

                // if loser is rebel kingdom or winner is rebel kingdom and is now dissolved
                if (kingdom1.IsRebelKingdom() || (kingdom2.IsRebelKingdom() && RebelFactionManager.Instance!.DeadRebelKingdoms.Contains(kingdom2)))
                    return;

                FormNonAggressionPactAction.Apply(kingdom1,
                                                  kingdom2,
                                                  bypassCosts: true,
                                                  customDurationInDays: Settings.Instance!.DeclareWarCooldownInDays,
                                                  queryPlayer: false);
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