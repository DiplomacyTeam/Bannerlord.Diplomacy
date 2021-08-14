using Diplomacy.DiplomaticAction.WarPeace;

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Diplomacy.Costs;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class WarExhaustionBehavior : CampaignBehaviorBase
    {
        private ILogger Log { get; }

        private WarExhaustionManager _warExhaustionManager;

        private static readonly TextObject _TDefeatTitle = new("{=BXluvRnJ}Bitter Defeat");

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

            foreach (var kingdom in Kingdom.All.ToList())
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
            var hasFiefsRemaining = kingdom.Fiefs.Count > 0;

            foreach (var targetKingdom in FactionManager.GetEnemyKingdoms(kingdom).ToList())
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

                        if (hasFiefsRemaining)
                        {
                            InformationManager.ShowInquiry(new InquiryData(
                                _TDefeatTitle.ToString(),
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
                            InformationManager.ShowInquiry(new InquiryData(
                            _TDefeatTitle.ToString(),
                            new TextObject("{=ghZCj7hb}With your final stronghold falling to your enemies, you can no longer continue the fight with {ENEMY_KINGDOM}. You must accept terms of defeat and pay war reparations of {DENARS} denars. The shame of defeat will also cost you {INFLUENCE} influence.", strArgs).ToString(),
                            true,
                            false,
                            GameTexts.FindText("str_ok").ToString(),
                            null,
                            () => KingdomPeaceAction.ApplyPeace(kingdom, targetKingdom),
                            null), true);
                        }
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
                var thirdPhase = StoryMode.StoryMode.Current.MainStoryLine?.ThirdPhase;
                isValidQuestState = thirdPhase is null || !thirdPhase.OppositionKingdoms.Contains(opposingKingdom);
            }

            return isValidQuestState;
        }
    }
}
