using Diplomacy.DiplomaticAction;
using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.DiplomaticAction.NonAggressionPact;
using Diplomacy.Extensions;

using Microsoft.Extensions.Logging;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class DiplomaticAgreementBehavior : CampaignBehaviorBase
    {
        private DiplomaticAgreementManager _diplomaticAgreementManager;

        public DiplomaticAgreementBehavior()
        {
            _diplomaticAgreementManager = new();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, UpdateDiplomaticAgreements);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, ConsiderDiplomaticAgreements);
            Events.AllianceFormed.AddNonSerializedListener(this, ExpireNonAggressionPact);
        }

        private void ConsiderDiplomaticAgreements(Clan clan)
        {
            // only apply to kingdom leader clans
            if (clan.MapFaction.IsKingdomFaction && clan.MapFaction.Leader == clan.Leader && !clan.Leader.IsHumanPlayerCharacter)
            {
                ConsiderNonAggressionPact((Kingdom)clan.MapFaction);
            }
        }

        private void ConsiderNonAggressionPact(Kingdom proposingKingdom)
        {
            if (MBRandom.RandomFloat < 0.05f)
            {
                var proposedKingdom = Kingdom.All
                    .Except(new Kingdom[] { proposingKingdom })?
                    .Where(kingdom => NonAggressionPactConditions.Instance.CanApply(proposingKingdom, kingdom))
                    .Where(kingdom => NonAggressionPactScoringModel.Instance.ShouldFormBidirectional(proposingKingdom, kingdom))
                    .OrderByDescending(kingdom => kingdom.GetExpansionism()).FirstOrDefault();

                if (proposedKingdom is not null)
                {
                    LogFactory.Get<DiplomaticAgreementBehavior>()
                        .LogTrace($"[{CampaignTime.Now}] {proposingKingdom.Name} decided to form a NAP with {proposedKingdom.Name}.");
                    FormNonAggressionPactAction.Apply(proposingKingdom, proposedKingdom);
                }
            }
        }

        private void UpdateDiplomaticAgreements()
        {
            DiplomaticAgreementManager.Instance.Agreements.Values
            .SelectMany(x => x)
            .ToList()
            .ForEach(x => x.TryExpireNotification());
        }

        private void ExpireNonAggressionPact(AllianceEvent obj)
        {
            if (DiplomaticAgreementManager.Instance.HasNonAggressionPact(obj.Kingdom, obj.OtherKingdom, out var pactAgreement))
                pactAgreement.Expire();
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_diplomaticAgreementManager", ref _diplomaticAgreementManager);

            if (dataStore.IsLoading)
            {
                _diplomaticAgreementManager ??= new();
                _diplomaticAgreementManager.Sync();
            }
        }
    }
}
