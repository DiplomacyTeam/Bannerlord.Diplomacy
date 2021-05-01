using Diplomacy.DiplomaticAction.WarPeace;

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

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
                    if (kingdom.Leader.IsHumanPlayerCharacter)
                    {
                        var diplomacyCost = DiplomacyCostCalculator.DetermineCostForMakingPeace(kingdom, targetKingdom, true);
                        var strArgs = new Dictionary<string,object>() { 
                            { "DENARS", diplomacyCost.GoldCost.Value },
                            { "INFLUENCE", diplomacyCost.InfluenceCost.Value },
                            { "ENEMY_KINGDOM", targetKingdom.Name } 
                        };
                        InformationManager.ShowInquiry(new InquiryData(
                            new TextObject("{=BXluvRnJ}Bitter Defeat").ToString(),
                            new TextObject("{=vLfbqXjq}Your armies and people are exhausted from the conflict with {ENEMY_KINGDOM} and have given up the fight. You must accept terms of defeat and pay war reparations of {DENARS} denars. The shame of defeat will also cost you {INFLUENCE} influence.", strArgs).ToString(),
                            true,
                            false,
                            GameTexts.FindText("str_ok").ToString(),
                            null,
                            () => KingdomPeaceAction.ApplyPeace(kingdom, targetKingdom),
                            null), true);
                    }
                    else
                    {
                        Log.LogTrace($"[{CampaignTime.Now}] {kingdom.Name}, due to max war exhaustion, will peace out with {targetKingdom.Name}.");
                        KingdomPeaceAction.ApplyPeace(kingdom, targetKingdom);
                    }
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
