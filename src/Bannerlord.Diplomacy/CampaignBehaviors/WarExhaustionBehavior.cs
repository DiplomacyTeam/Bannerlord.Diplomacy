using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.Extensions;

using Microsoft.Extensions.Logging;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class WarExhaustionBehavior : CampaignBehaviorBase
    {
        private ILogger Log { get; }

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

            CampaignEvents.WarDeclared.AddNonSerializedListener(this, RegisterWarExhaustionMultiplier);
            CampaignEvents.MakePeace.AddNonSerializedListener(this, ClearWarExhaustion);
        }

        private void ClearWarExhaustion(IFaction faction1, IFaction faction2)
        {
            if (faction1 is Kingdom kingdom1 && faction2 is Kingdom kingdom2)
            {
                _warExhaustionManager.ClearWarExhaustion(kingdom1, kingdom2);
                _warExhaustionManager.ClearWarExhaustion(kingdom2, kingdom1);
            }
        }

        private void RegisterWarExhaustionMultiplier(IFaction faction1, IFaction faction2)
        {
            if (faction1 is Kingdom kingdom1 && faction2 is Kingdom kingdom2)
            {
                _warExhaustionManager.RegisterWarExhaustionMultiplier(kingdom1, kingdom2);
            }
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

            foreach (var kingdom in KingdomExtensions.AllActiveKingdoms.ToList())
            {
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
            {
                if (oldOwnerKingdom.Fiefs.Any())
                {
                    _warExhaustionManager.AddSiegeWarExhaustion(oldOwnerKingdom, newOwnerKingdom);
                }
                else
                {
                    var warKingdoms = oldOwnerKingdom.Stances
                        .Where(x => x.IsAtWar && x.Faction1.IsKingdomFaction && x.Faction2.IsKingdomFaction)
                        .Select(x => x.Faction1 == oldOwnerKingdom ? x.Faction2 : x.Faction1)
                        .ToList();

                    foreach (var faction in warKingdoms)
                    {
                        var warKingdom = (Kingdom) faction;
                        _warExhaustionManager.AddOccupiedWarExhaustion(oldOwnerKingdom, warKingdom);
                    }
                }
            }
        }

        private void ConsiderPeaceActions(Kingdom kingdom)
        {
            foreach (var targetKingdom in FactionManager.GetEnemyKingdoms(kingdom).ToList())
            {
                if (_warExhaustionManager.HasMaxWarExhaustion(kingdom, targetKingdom) && IsValidQuestState(kingdom, targetKingdom))
                {
                    {
                        Log.LogTrace($"[{CampaignTime.Now}] {kingdom.Name}, due to max war exhaustion, will peace out with {targetKingdom.Name}.");
                        KingdomPeaceAction.ApplyPeace(kingdom, targetKingdom);
                    }
                }
            }
        }

        private bool IsValidQuestState(Kingdom kingdom1, Kingdom kingdom2)
        {
            var currentStoryMode = StoryMode.StoryModeManager.Current;
            // not in story mode
            if (currentStoryMode == null)
            {
                return true;
            }

            var isValidQuestState = true;
            var opposingKingdom = PlayerHelpers.GetOpposingKingdomIfPlayerKingdomProvided(kingdom1, kingdom2);

            if (opposingKingdom is not null)
            {
                var thirdPhase = currentStoryMode?.MainStoryLine?.ThirdPhase;
                isValidQuestState = thirdPhase is null || !thirdPhase.OppositionKingdoms.Contains(opposingKingdom);
            }

            return isValidQuestState;
        }
    }
}
