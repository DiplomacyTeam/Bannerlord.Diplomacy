using Diplomacy.DiplomaticAction;
using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.Event;
using Diplomacy.Extensions;
using System.Linq;
using Diplomacy.DiplomaticAction.Barter;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class DiplomaticAgreementBehavior : CampaignBehaviorBase
    {
        private const float BasePactChance = 0.05f;

        private DiplomaticAgreementManager _diplomaticAgreementManager;

        public DiplomaticAgreementBehavior()
        {
            _diplomaticAgreementManager = new();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, UpdateDiplomaticAgreements);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, ClanDailyTick);
            Events.AllianceFormed.AddNonSerializedListener(this, ExpireNonAggressionPact);
        }

        private void ClanDailyTick(Clan clan)
        {
            // only apply to kingdom leader clans
            if (clan.MapFaction.IsKingdomFaction && clan.MapFaction.Leader == clan.Leader && !clan.Leader.IsHumanPlayerCharacter)
            {
                ConsiderDiplomaticBarter(clan.Kingdom);
            }
        }

        private void ConsiderDiplomaticBarter(Kingdom proposingKingdom)
        {
            Kingdom kingdomToBarterWith = Kingdom.All.Where(x => x != proposingKingdom && !x.IsRebelKingdom() && !x.IsEliminated).GetRandomElementInefficiently();
            DiplomaticBarter barter = new(proposingKingdom, kingdomToBarterWith);
            barter.ExecuteAIBarter();
        }

        private void UpdateDiplomaticAgreements()
        {
            DiplomaticAgreementManager.Instance!.Agreements.Values
            .SelectMany(x => x)
            .ToList()
            .ForEach(x => x.TryExpireNotification());
        }

        private void ExpireNonAggressionPact(AllianceEvent obj)
        {
            if (DiplomaticAgreementManager.HasNonAggressionPact(obj.Kingdom, obj.OtherKingdom, out var pactAgreement))
                pactAgreement!.Expire();
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
