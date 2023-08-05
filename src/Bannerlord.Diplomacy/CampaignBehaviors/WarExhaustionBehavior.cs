using Diplomacy.WarExhaustion;

using Microsoft.Extensions.Logging;

using System;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

using static Diplomacy.WarExhaustion.WarExhaustionManager;

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
            CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, OnRaidCompleted);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
            CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this, OnPrisonerTaken);
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);

            CampaignEvents.WarDeclared.AddNonSerializedListener(this, RegisterWarExhaustion);
            CampaignEvents.MakePeace.AddNonSerializedListener(this, ClearWarExhaustion);

            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoadFinished);

#if v120 || v121 || v122 || v123
            CampaignEvents.CanKingdomBeDiscontinuedEvent.AddNonSerializedListener(this, CanKingdomBeDiscontinued);
#endif
        }

#if v120 || v121 || v122 || v123
        private void CanKingdomBeDiscontinued(Kingdom kingdom, ref bool result)
        {
            if (Settings.Instance!.EnableKingdomElimination)
                result = false; //TODO: Probably should move on to vanila kingdom dispatching when making peace
        }
#endif

#if v100 || v101 || v102 || v103
        private void ClearWarExhaustion(IFaction faction1, IFaction faction2)
#else
        private void ClearWarExhaustion(IFaction faction1, IFaction faction2, MakePeaceAction.MakePeaceDetail makePeaceDetail)
#endif
        {
            if (faction1 is Kingdom kingdom1 && faction2 is Kingdom kingdom2)
            {
                _warExhaustionManager.ClearWarExhaustion(kingdom1, kingdom2);
            }
        }

#if v100 || v101 || v102 || v103
        private void RegisterWarExhaustion(IFaction faction1, IFaction faction2)
#else
        private void RegisterWarExhaustion(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail declareWarDetail)
#endif
        {
            if (faction1 is Kingdom kingdom1 && faction2 is Kingdom kingdom2)
            {
                _warExhaustionManager.RegisterWarExhaustion(kingdom1, kingdom2);
            }
        }

#if v100 || v101 || v102 || v103
        private void OnRaidCompleted(BattleSideEnum battleSide, MapEvent mapEvent)
        {
            if (battleSide != BattleSideEnum.Attacker || !VerifyEventSides(mapEvent.AttackerSide.LeaderParty, mapEvent.DefenderSide.LeaderParty, out var attacker, out var defender))
                return;

            CreateKey(attacker!, defender!, out var kingdoms);
            if (kingdoms is not null)
                _warExhaustionManager.AddRaidWarExhaustion(kingdoms, mapEvent);
        }
#else
        private void OnRaidCompleted(BattleSideEnum battleSide, RaidEventComponent raidEventComponent)
        {
            if (battleSide != BattleSideEnum.Attacker || !VerifyEventSides(raidEventComponent.AttackerSide.LeaderParty, raidEventComponent.DefenderSide.LeaderParty, out var attacker, out var defender))
                return;

            CreateKey(attacker!, defender!, out var kingdoms);
            if (kingdoms is not null)
                _warExhaustionManager.AddRaidWarExhaustion(kingdoms, raidEventComponent);
        }
#endif

        private void OnMapEventEnded(MapEvent mapEvent)
        {
            if (!VerifyEventSides(mapEvent.AttackerSide.LeaderParty, mapEvent.DefenderSide.LeaderParty, out var attacker, out var defender))
                return;

            CreateKey(attacker!, defender!, out var kingdoms);
            if (kingdoms is null)
                return;

            //Casualties
            _warExhaustionManager.AddCasualtyWarExhaustion(kingdoms, mapEvent);

            if (mapEvent.WinningSide == BattleSideEnum.None || mapEvent.DefeatedSide == BattleSideEnum.None)
            {
                return;
            }

            //Sieges and occupation
            if (mapEvent.MapEventSettlement is not null && mapEvent.MapEventSettlement.IsFortification && mapEvent.BattleState == BattleState.AttackerVictory && mapEvent.EventType == MapEvent.BattleTypes.Siege)
            {
                _warExhaustionManager.AddSiegeWarExhaustion(kingdoms, mapEvent);
                if (!defender!.Fiefs.Any(f => f != mapEvent.MapEventSettlement.Town))
                {
                    _warExhaustionManager.AddOccupiedWarExhaustion(kingdoms);
                }
            }

            //Caravans
            var winningSide = mapEvent.GetMapEventSide(mapEvent.WinningSide);
            var defeatedSide = mapEvent.GetMapEventSide(mapEvent.DefeatedSide);
            //a separate key combination for clearer who gets what logic
            if (!VerifyEventSides(winningSide.LeaderParty, defeatedSide.LeaderParty, out var winnerKingdom, out var defeatedKingdom))
                return;
            CreateKey(winnerKingdom!, defeatedKingdom!, out kingdoms);
            if (kingdoms is null)
                return;

            var defeatedCaravanParties = defeatedSide.Parties.Where(p => p.Party.IsMobile && p.Party.MobileParty.IsCaravan).Select(p => p.Party.MobileParty).ToList();
            defeatedCaravanParties.ForEach(cp => _warExhaustionManager.AddCaravanRaidWarExhaustion(kingdoms, cp, winningSide));
        }

        private void OnPrisonerTaken(PartyBase party, Hero hero)
        {
            if (!VerifyEventSides(party, hero, out var effector, out var effected))
                return;

            CreateKey(effector!, effected!, out var kingdoms);
            if (kingdoms is null)
                return;

            _warExhaustionManager.AddHeroImprisonedWarExhaustion(kingdoms, hero, party.Name ?? effector!.Name);
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
        {
            if (!VerifyEventSides(killer, victim, out var effector, out var effected))
                return;

            CreateKey(effector!, effected!, out var kingdoms);
            if (kingdoms is null)
                return;

            switch (detail)
            {
                case KillCharacterAction.KillCharacterActionDetail.None:
                case KillCharacterAction.KillCharacterActionDetail.DiedInLabor:
                case KillCharacterAction.KillCharacterActionDetail.DiedOfOldAge:
                    return;
            }

            _warExhaustionManager.AddHeroPerishedWarExhaustion(kingdoms, victim, killer.PartyBelongedTo.Name ?? effector!.Name, detail);
        }


        private static bool VerifyEventSides(PartyBase attackerSideLeaderParty, PartyBase defenderSideLeaderParty, out Kingdom? attacker, out Kingdom? defender)
        {
            if ((attackerSideLeaderParty.IsMobile && (attackerSideLeaderParty.MobileParty?.IsBandit ?? false)) || (defenderSideLeaderParty.IsMobile && (defenderSideLeaderParty.MobileParty?.IsBandit ?? false))
                || attackerSideLeaderParty.MapFaction == null || attackerSideLeaderParty.MapFaction.IsBanditFaction
                || defenderSideLeaderParty.MapFaction == null || defenderSideLeaderParty.MapFaction.IsBanditFaction
                || attackerSideLeaderParty.MapFaction is not Kingdom attackerKingdom || defenderSideLeaderParty.MapFaction is not Kingdom defenderKingdom
                || attackerKingdom == defenderKingdom)
            {
                attacker = null;
                defender = null;
                return false;
            }
            else
            {
                attacker = attackerKingdom;
                defender = defenderKingdom;
                return true;
            }
        }

        private static bool VerifyEventSides(PartyBase effectorSideParty, Hero effectedSideHero, out Kingdom? effector, out Kingdom? effected)
        {
            if (effectorSideParty.IsMobile && (effectorSideParty.MobileParty?.IsBandit ?? false)
                || effectorSideParty.MapFaction == null || effectorSideParty.MapFaction.IsBanditFaction
                || effectedSideHero.MapFaction == null || effectedSideHero.MapFaction.IsBanditFaction
                || effectorSideParty.MapFaction is not Kingdom effectorKingdom || effectedSideHero.MapFaction is not Kingdom effectedKingdom
                || effectorKingdom == effectedKingdom)
            {
                effector = null;
                effected = null;
                return false;
            }
            else
            {
                effector = effectorKingdom;
                effected = effectedKingdom;
                return true;
            }
        }

        private static bool VerifyEventSides(Hero? effectorSideHero, Hero effectedSideHero, out Kingdom? effector, out Kingdom? effected)
        {
            if (effectorSideHero == null || effectorSideHero.MapFaction == null || effectorSideHero.MapFaction.IsBanditFaction
                || effectedSideHero.MapFaction == null || effectedSideHero.MapFaction.IsBanditFaction
                || effectorSideHero.MapFaction is not Kingdom effectorKingdom || effectedSideHero.MapFaction is not Kingdom effectedKingdom
                || effectorKingdom == effectedKingdom)
            {
                effector = null;
                effected = null;
                return false;
            }
            else
            {
                effector = effectorKingdom;
                effected = effectedKingdom;
                return true;
            }
        }

        private void OnDailyTick()
        {
            _warExhaustionManager.UpdateDailyWarExhaustionForAllKingdoms();
            _warExhaustionManager.ProcessWarExhaustion(Log);
        }

        public void OnGameLoadFinished() => _warExhaustionManager.OnAfterSaveLoaded();

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_warExhaustionManager", ref _warExhaustionManager);

            if (dataStore.IsLoading)
            {
                _warExhaustionManager ??= new();
                _warExhaustionManager.Sync();
            }
        }
    }
}