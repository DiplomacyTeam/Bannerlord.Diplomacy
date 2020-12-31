using Diplomacy.DiplomaticAction.WarPeace;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace Diplomacy.CampaignEventBehaviors
{
    public class WarExhaustionBehavior : CampaignBehaviorBase
    {
        private WarExhaustionManager _warExhaustionManager;

        public WarExhaustionBehavior()
        {
            _warExhaustionManager = new WarExhaustionManager();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, RaidCompleted);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, MapEventEnded);
        }

        private void RaidCompleted(BattleSideEnum battleSide, MapEvent mapEvent)
        {
            if (battleSide != BattleSideEnum.Attacker)
            {
                return;
            }

            Kingdom attackerKingdom = mapEvent.AttackerSide.LeaderParty.MapFaction as Kingdom;
            Kingdom defenderKingdom = mapEvent.DefenderSide.LeaderParty.MapFaction as Kingdom;

            if (attackerKingdom == null || defenderKingdom == null)
            {
                return;
            }

            _warExhaustionManager.AddRaidWarExhaustion(defenderKingdom, attackerKingdom);
        }

        private void MapEventEnded(MapEvent mapEvent)
        {
            Kingdom attackerSide = mapEvent.AttackerSide.LeaderParty.MapFaction as Kingdom;
            Kingdom defenderSide = mapEvent.DefenderSide.LeaderParty.MapFaction as Kingdom;

            if (attackerSide == null || defenderSide == null)
            {
                return;
            }

            _warExhaustionManager.AddCasualtyWarExhaustion(attackerSide, defenderSide, mapEvent.AttackerSide.Casualties);
            _warExhaustionManager.AddCasualtyWarExhaustion(defenderSide, attackerSide, mapEvent.DefenderSide.Casualties);
        }

        private void OnDailyTick()
        {
            _warExhaustionManager.UpdateDailyWarExhaustionForAllKingdoms();

            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (kingdom.Leader.IsHumanPlayerCharacter)
                {
                    continue;
                }
                ConsiderPeaceActions(kingdom);
            }
        }

        public void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (detail != ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.BySiege)
            {
                return;
            }

            Kingdom previousOwnerKingdom = oldOwner.MapFaction as Kingdom;
            Kingdom newOwnerKingdom = newOwner.MapFaction as Kingdom;

            if (previousOwnerKingdom != null && newOwnerKingdom != null && previousOwnerKingdom != newOwnerKingdom)
            {
                _warExhaustionManager.AddSiegeWarExhaustion(previousOwnerKingdom, newOwnerKingdom);
            }
        }

        private void ConsiderPeaceActions(Kingdom kingdom)
        {
            foreach (Kingdom targetKingdom in FactionManager.GetEnemyKingdoms(kingdom))
            {
                if (_warExhaustionManager.HasMaxWarExhaustion(kingdom, targetKingdom) && IsValidQuestState(kingdom, targetKingdom))
                {
                    KingdomPeaceAction.ApplyPeace(kingdom, targetKingdom);
                }
            }
        }

        private bool IsValidQuestState(Kingdom kingdom1, Kingdom kingdom2)
        {
            bool isValidQuestState = true;
            Kingdom opposingKingdom = PlayerHelpers.GetOpposingKingdomIfPlayerKingdomProvided(kingdom1, kingdom2);
            if (opposingKingdom != null)
            {
                ThirdPhase thirdPhase = StoryMode.StoryMode.Current.MainStoryLine.ThirdPhase;
                isValidQuestState = thirdPhase == null || !thirdPhase.OppositionKingdoms.Contains(opposingKingdom);
            }
            return isValidQuestState;
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_warExhaustionManager", ref _warExhaustionManager);
            if (dataStore.IsLoading)
            {
                if (_warExhaustionManager == null)
                {
                    this._warExhaustionManager = new WarExhaustionManager();
                }
                this._warExhaustionManager.Sync();
            }
        }
    }
}
