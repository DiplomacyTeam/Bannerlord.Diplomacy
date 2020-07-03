using DiplomacyFixes.DiplomaticAction;
using DiplomacyFixes.DiplomaticAction.WarPeace;
using DiplomacyFixes.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    class ExpansionismBehavior : CampaignBehaviorBase
    {
        private ExpansionismManager _expansionismManager;

        public ExpansionismBehavior()
        {
            this._expansionismManager = new ExpansionismManager();
        }
        public override void RegisterEvents()
        {
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, UpdateExpasionismScore);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, UpdateExpansionismDecay);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
        }

        private void DailyTick()
        {
            if (MBRandom.RandomFloat < 0.05f)
            {
                ConsiderCoalition();
            }
        }

        private void ConsiderCoalition()
        {
            Kingdom kingdomWithCriticalExpansionism = Kingdom.All.Where(kingdom => kingdom.GetExpansionism() > _expansionismManager.CriticalExpansionism).OrderByDescending(kingdom => kingdom.GetExpansionism()).FirstOrDefault();
            if (kingdomWithCriticalExpansionism != null)
            {
                List<Kingdom> potentialCoaliationMembers =
                    Kingdom.All.Except(new Kingdom[] { kingdomWithCriticalExpansionism })
                    .Where(kingdom => WarAndPeaceConditions.CanDeclareWar(kingdom, kingdomWithCriticalExpansionism)).ToList();
                if (Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom.Leader.IsHumanPlayerCharacter)
                {
                    potentialCoaliationMembers.Remove(Clan.PlayerClan.Kingdom);
                }


                List<Kingdom> currentEnemies = Kingdom.All.Where(kingdom => kingdom.IsAtWarWith(kingdomWithCriticalExpansionism)).ToList();
                List<Kingdom> newEnemies = new List<Kingdom>();

                potentialCoaliationMembers.Shuffle();
                foreach (Kingdom potenatialCoalitionMember in potentialCoaliationMembers)
                {
                    if (kingdomWithCriticalExpansionism.TotalStrength > currentEnemies.Select(kingdom => kingdom.TotalStrength).Sum())
                    {
                        newEnemies.Add(potenatialCoalitionMember);
                        DeclareWarAction.Apply(potenatialCoalitionMember, kingdomWithCriticalExpansionism);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void UpdateExpansionismDecay(Clan clan)
        {
            if(clan.MapFaction.IsKingdomFaction && clan.Leader == clan.MapFaction.Leader)
            {
                this._expansionismManager.ApplyExpansionismDecay(clan.MapFaction);
            }
        }

        private void UpdateExpasionismScore(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (newOwner.MapFaction != oldOwner.MapFaction && newOwner.MapFaction.IsKingdomFaction && detail == ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.BySiege)
            {
                this._expansionismManager.AddSiegeScore(newOwner.MapFaction);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_expansionismManager", ref _expansionismManager);
            if (dataStore.IsLoading)
            {
                this._expansionismManager.Sync();
            }
        }
    }
}
