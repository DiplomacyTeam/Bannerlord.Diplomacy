using DiplomacyFixes.DiplomaticAction.NonAggressionPact;
using DiplomacyFixes.DiplomaticAction.WarPeace;
using DiplomacyFixes.Extensions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

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
            if (Settings.Instance.EnableCoalitions && MBRandom.RandomFloat < Settings.Instance.CoalitionChancePercentage / 100)
            {
                ConsiderCoalition();
            }
        }

        private void ConsiderCoalition()
        {
            Kingdom kingdomWithCriticalExpansionism = Kingdom.All
                .Where(kingdom => kingdom.GetExpansionism() > _expansionismManager.CriticalExpansionism && kingdom.IsStrong())
                .OrderByDescending(kingdom => kingdom.GetExpansionism())
                .FirstOrDefault();
            if (kingdomWithCriticalExpansionism != null)
            {
                List<Kingdom> potentialCoalitionMembers =
                    Kingdom.All.Except(new Kingdom[] { kingdomWithCriticalExpansionism })
                    .Where(kingdom => DeclareWarConditions.Instance.CanApply(kingdom, kingdomWithCriticalExpansionism, bypassCosts:true))
                    .ToList();

                List<Kingdom> oldCoalitionMembers = Kingdom.All.Where(kingdom => kingdom.IsAtWarWith(kingdomWithCriticalExpansionism)).ToList();
                List<Kingdom> newCoalitionMembers = new List<Kingdom>();

                potentialCoalitionMembers.Shuffle();
                foreach (Kingdom potentialCoalitionMember in potentialCoalitionMembers)
                {
                    if (kingdomWithCriticalExpansionism.GetAllianceStrength() <= CalculateStrength(newCoalitionMembers, oldCoalitionMembers))
                    {
                        break;
                    }

                    if (Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom.Leader.IsHumanPlayerCharacter && Clan.PlayerClan.Kingdom == potentialCoalitionMember)
                    {
                        // show inquiry
                        continue;
                    }
                    HashSet<Kingdom> alliesIncluded = new HashSet<Kingdom>() { potentialCoalitionMember };
                    alliesIncluded.UnionWith(potentialCoalitionMember.GetAlliedKingdoms().Where(alliedKingdom => DeclareWarConditions.Instance.CanApply(alliedKingdom, kingdomWithCriticalExpansionism, bypassCosts:true)));
                    newCoalitionMembers.AddRange(alliesIncluded);
                }

                if (newCoalitionMembers.IsEmpty())
                {
                    return;
                }

                foreach (Kingdom kingdom in newCoalitionMembers)
                {
                    DeclareWarAction.Apply(kingdom, kingdomWithCriticalExpansionism);
                }

                List<Kingdom> allCoalitionMembers = oldCoalitionMembers.Union(newCoalitionMembers).ToList();
                HashSet<FactionMapping> pactsToForm = new HashSet<FactionMapping>();

                foreach (Kingdom kingdom in allCoalitionMembers)
                {
                    foreach (Kingdom otherKingdom in allCoalitionMembers.Where(curKingdom => curKingdom != kingdom))
                    {
                        pactsToForm.Add(new FactionMapping(kingdom, otherKingdom));
                    }
                }

                foreach (FactionMapping mapping in pactsToForm)
                {
                    if (mapping.Faction1.IsAtWarWith(mapping.Faction2))
                    {
                        if (MakePeaceConditions.Instance.CanApply(mapping.Faction1 as Kingdom, mapping.Faction2 as Kingdom, bypassCosts: true))
                        {
                            KingdomPeaceAction.ApplyPeace(mapping.Faction1 as Kingdom, mapping.Faction2 as Kingdom, bypassCosts:true);
                        }
                        else continue;
                    }

                    if (NonAggressionPactConditions.Instance.CanApply(mapping.Faction1 as Kingdom, mapping.Faction2 as Kingdom, bypassCosts:true)) {
                        FormNonAggressionPactAction.Apply(mapping.Faction1 as Kingdom, mapping.Faction2 as Kingdom, bypassCosts:true);
                    }
                }

                TextObject coalitionkingdomname = newCoalitionMembers.OrderByDescending(kingdom => kingdom.TotalStrength).FirstOrDefault()?.Name;
                TextObject textField = new TextObject("Calradia grows wary of the inexorable expansion of the {EXPANSIONIST_KINGDOM}. A coalition led by the {COALITION_KINGDOM} has been formed to fight them!");
                textField.SetTextVariable("EXPANSIONIST_KINGDOM", kingdomWithCriticalExpansionism.Name);
                textField.SetTextVariable("COALITION_KINGDOM", coalitionkingdomname);

                InformationManager.ShowInquiry(
                    new InquiryData(
                        new TextObject("Coalition Formed").ToString(),
                        textField.ToString(),
                        true,
                        false,
                        GameTexts.FindText("str_ok", null).ToString(),
                        null,
                        null,
                        null,
                        ""), true);
            }
        }

        private float CalculateStrength(List<Kingdom> newCoalitionMembers, List<Kingdom> oldCoalitionMembers)
        {
            List<Kingdom> allKingdomStrengths = new List<Kingdom>();
            allKingdomStrengths.AddRange(newCoalitionMembers);
            allKingdomStrengths.AddRange(oldCoalitionMembers);

            foreach (Kingdom kingdom in newCoalitionMembers)
            {
                allKingdomStrengths.AddRange(kingdom.GetAlliedKingdoms());
            }

            return allKingdomStrengths.Select(kingdom => kingdom.TotalStrength).Sum();
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
