using Diplomacy.DiplomaticAction.WarPeace;

using Microsoft.Extensions.Logging;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class WarExhaustionBehavior : CampaignBehaviorBase
    {
        private ILogger Log { get; init; }

        private WarExhaustionManager _warExhaustionManager;

        public WarExhaustionBehavior()
        {
            Log = LogFactory.Get<WarExhaustionBehavior>();
            _warExhaustionManager = new();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, OnRaidCompleted);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_warExhaustionManager", ref _warExhaustionManager);

            if (dataStore.IsLoading)
            {
                _warExhaustionManager ??= new();
                _warExhaustionManager.Sync();
            }
        }

        private void OnRaidCompleted(BattleSideEnum battleSide, MapEvent mapEvent)
        {
            if (battleSide != BattleSideEnum.Attacker
                || mapEvent.AttackerSide.LeaderParty.MapFaction is not Kingdom attacker
                || mapEvent.DefenderSide.LeaderParty.MapFaction is not Kingdom defender)
                return;

            _warExhaustionManager.AddRaidWarExhaustion(defender, attacker);
        }

        private void OnMapEventEnded(MapEvent mapEvent)
        {
            if (mapEvent.AttackerSide.LeaderParty.MapFaction is not Kingdom attacker
                || mapEvent.DefenderSide.LeaderParty.MapFaction is not Kingdom defender)
                return;

            _warExhaustionManager.AddCasualtyWarExhaustion(attacker, defender, mapEvent.AttackerSide.Casualties);
            _warExhaustionManager.AddCasualtyWarExhaustion(defender, attacker, mapEvent.DefenderSide.Casualties);
        }

        private void OnDailyTick()
        {
            _warExhaustionManager.UpdateDailyWarExhaustionForAllKingdoms();

            foreach (var kingdom in Kingdom.All)
            {
                if (kingdom.Leader.IsHumanPlayerCharacter)
                    continue;

                ConsiderPeaceActions(kingdom);
            }
        }

        public void OnSettlementOwnerChanged(Settlement settlement,
                                             bool openToClaim,
                                             Hero newOwner,
                                             Hero oldOwner,
                                             Hero capturerHero,
                                             ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (detail != ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.BySiege)
                return;

            if (newOwner.MapFaction == oldOwner.MapFaction)
                return;

            if (oldOwner.MapFaction is Kingdom oldOwnerKingdom && newOwner.MapFaction is Kingdom newOwnerKingdom)
                _warExhaustionManager.AddSiegeWarExhaustion(oldOwnerKingdom, newOwnerKingdom);
        }

        private void ConsiderPeaceActions(Kingdom kingdom)
        {
            foreach (var targetKingdom in FactionManager.GetEnemyKingdoms(kingdom))
            {
                if (_warExhaustionManager.HasMaxWarExhaustion(kingdom, targetKingdom) && IsValidQuestState(kingdom, targetKingdom))
                {
                    Log.LogTrace($"[{CampaignTime.Now}] {kingdom.Name}, due to max war exhaustion, will peace out with {targetKingdom.Name}.");
                    KingdomPeaceAction.ApplyPeace(kingdom, targetKingdom);
                }
            }
        }

        private bool IsValidQuestState(Kingdom kingdom1, Kingdom kingdom2)
        {
            // not in story mode
            if (StoryMode.StoryMode.Current == null)
            {
                return true;
            } 

            var isValidQuestState = true;
            var opposingKingdom = PlayerHelpers.GetOpposingKingdomIfPlayerKingdomProvided(kingdom1, kingdom2);

            if (opposingKingdom is not null)
            {
                var thirdPhase = StoryMode.StoryMode.Current?.MainStoryLine?.ThirdPhase;
                isValidQuestState = thirdPhase is null || !thirdPhase.OppositionKingdoms.Contains(opposingKingdom);
            }

            return isValidQuestState;
        }
    }
}
