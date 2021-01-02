using Diplomacy.DiplomaticAction.WarPeace;

using Microsoft.Extensions.Logging;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace Diplomacy.CampaignBehaviors
{
    public class WarExhaustionBehavior : CampaignBehaviorBase
    {
        private WarExhaustionManager _warExhaustionManager;

        public WarExhaustionBehavior()
        {
            _warExhaustionManager = new();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, RaidCompleted);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, MapEventEnded);
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

        private void RaidCompleted(BattleSideEnum battleSide, MapEvent mapEvent)
        {
            if (battleSide != BattleSideEnum.Attacker)
                return;

            if (mapEvent.AttackerSide.LeaderParty.MapFaction is Kingdom attacker
                && mapEvent.DefenderSide.LeaderParty.MapFaction is Kingdom defender)
            {
                _warExhaustionManager.AddRaidWarExhaustion(defender, attacker);
            }
        }

        private void MapEventEnded(MapEvent mapEvent)
        {
            if (mapEvent.AttackerSide.LeaderParty.MapFaction is Kingdom attacker
                && mapEvent.DefenderSide.LeaderParty.MapFaction is Kingdom defender)
            {
                _warExhaustionManager.AddCasualtyWarExhaustion(attacker, defender, mapEvent.AttackerSide.Casualties);
                _warExhaustionManager.AddCasualtyWarExhaustion(defender, attacker, mapEvent.DefenderSide.Casualties);
            }
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

            if (oldOwner.MapFaction is Kingdom prevOwnerKingdom
                && newOwner.MapFaction is Kingdom newOwnerKingdom
                && prevOwnerKingdom != newOwnerKingdom)
            {
                _warExhaustionManager.AddSiegeWarExhaustion(prevOwnerKingdom, newOwnerKingdom);
            }
        }

        private void ConsiderPeaceActions(Kingdom kingdom)
        {
            foreach (var targetKingdom in FactionManager.GetEnemyKingdoms(kingdom))
            {
                if (_warExhaustionManager.HasMaxWarExhaustion(kingdom, targetKingdom) && IsValidQuestState(kingdom, targetKingdom))
                {
                    Log.Get<WarExhaustionBehavior>()
                        .LogTrace($"[{CampaignTime.Now}] {kingdom.Name}, due to max war exhaustion, will peace out with {targetKingdom.Name}.");

                    KingdomPeaceAction.ApplyPeace(kingdom, targetKingdom);
                }
            }
        }

        private bool IsValidQuestState(Kingdom kingdom1, Kingdom kingdom2)
        {
            var isValidQuestState = true;
            var opposingKingdom = PlayerHelpers.GetOpposingKingdomIfPlayerKingdomProvided(kingdom1, kingdom2);
            if (opposingKingdom is not null)
            {
                var thirdPhase = StoryMode.StoryMode.Current.MainStoryLine.ThirdPhase;
                isValidQuestState = thirdPhase is null || !thirdPhase.OppositionKingdoms.Contains(opposingKingdom);
            }
            return isValidQuestState;
        }
    }
}
