using Diplomacy.CivilWar.Actions;
using Diplomacy.CivilWar.Factions;
using Diplomacy.Helpers;
using Diplomacy.WarExhaustion;

using JetBrains.Annotations;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.SaveSystem;

namespace Diplomacy.CivilWar
{
    internal sealed class RebelFactionManager
    {
        public static RebelFactionManager? Instance { get; private set; }

        [SaveableProperty(1)]
        [UsedImplicitly]
        public Dictionary<Kingdom, List<RebelFaction>> RebelFactions { get; private set; }
        [SaveableProperty(2)]
        [UsedImplicitly]
        public List<Kingdom> DeadRebelKingdoms { get; private set; }
        [SaveableProperty(3)]
        [UsedImplicitly]
        public Dictionary<Kingdom, CampaignTime> LastCivilWar { get; private set; }

        public RebelFactionManager()
        {
            RebelFactions = new();
            DeadRebelKingdoms = new();
            LastCivilWar = new();
            Instance = this;
        }

        internal void Sync()
        {
            Instance = this;
        }

        public static void RegisterRebelFaction(RebelFaction rebelFaction)
        {
            var kingdom = rebelFaction.ParentKingdom;

            if (Instance!.RebelFactions.TryGetValue(kingdom, out var rebelFactions))
            {
                // if we're starting a secession faction, remove this clan from other secession factions
                if (rebelFaction.RebelDemandType == RebelDemandType.Secession)
                {
                    var otherSecessionFactions = rebelFactions.Where(x => x.RebelDemandType == RebelDemandType.Secession && x.Clans.Contains(rebelFaction.SponsorClan));
                    foreach (var faction in otherSecessionFactions)
                    {
                        faction.RemoveClan(rebelFaction.SponsorClan);
                    }
                }
                Instance!.RebelFactions[kingdom].Add(rebelFaction);
            }
            else
            {
                List<RebelFaction> newRebelFactions = new() { rebelFaction };
                Instance!.RebelFactions[kingdom] = newRebelFactions;
            }
        }

        public static void DestroyRebelFaction(RebelFaction rebelFaction, bool rebelKingdomSurvived = false)
        {
            if (rebelFaction.AtWar && rebelFaction.RebelKingdom is not null)
            {
                Instance!.LastCivilWar[rebelFaction.ParentKingdom] = CampaignTime.Now;

                if (rebelKingdomSurvived)
                {
                    Instance!.LastCivilWar[rebelFaction.RebelKingdom!] = CampaignTime.Now;
                }
                else
                {
                    Instance!.DeadRebelKingdoms.Add(rebelFaction.RebelKingdom!);
                }
            }
            Instance!.RebelFactions[rebelFaction.ParentKingdom].Remove(rebelFaction);
        }

        public static IEnumerable<RebelFaction> GetRebelFaction(Kingdom kingdom)
        {
            if (kingdom is null)
            {
                return Enumerable.Empty<RebelFaction>();
            }

            if (Instance!.RebelFactions.TryGetValue(kingdom, out var rebelFactions))
            {
                return new List<RebelFaction>(rebelFactions);
            }
            else
            {
                return Enumerable.Empty<RebelFaction>();
            }
        }

        public static RebelFaction? GetRebelFactionForRebelKingdom(Kingdom rebelKingdom)
        {
            return AllRebelFactions.Values.SelectMany(x => x).FirstOrDefault(rf => rebelKingdom == rf.RebelKingdom);
        }

        public static Kingdom GetCivilWarLoser(Kingdom kingdomMakingPeace, Kingdom otherKingdom)
        {
            return (WarExhaustionManager.Instance?.GetWarResult(kingdomMakingPeace, otherKingdom) ?? WarExhaustionManager.WarResult.None) switch
            {
                WarExhaustionManager.WarResult.Tie when kingdomMakingPeace.Fiefs.Any() => TributeHelper.GetBaseValueForTrubute(kingdomMakingPeace, otherKingdom) < 0 ? otherKingdom : kingdomMakingPeace,
                >= WarExhaustionManager.WarResult.PyrrhicVictory => otherKingdom,
                _ => kingdomMakingPeace,
            };
        }

        internal void OnAfterSaveLoaded()
        {
            //Remove factions of dead kingdoms
            var keysToRemove = AllRebelFactions.Keys.Where(k => k.IsEliminated).ToList();
            foreach (var keyToRemove in keysToRemove)
            {
                RebelFactions.Remove(keyToRemove);
            }
            //Account for eliminated clans
            var factionsToClean = AllRebelFactions.Values.SelectMany(x => x).Where(x => x.Clans.Any(clan => clan.IsEliminated)).ToList();
            foreach (var faction in factionsToClean)
            {
                if (faction.Clans.All(clan => clan.IsEliminated))
                {
                    //Destroy dead factions
                    DestroyRebelFaction(faction);
                    continue;
                }
                foreach (var clan in faction.Clans.ToList())
                {
                    //Clear rest of the factions from dead clans
                    if (clan.IsEliminated) faction.RemoveClan(clan);
                }
            }
            //Fix factions that count as dead but not actually dead
            var kingdomsToReanimate = DeadRebelKingdoms.Where(k => !k.IsEliminated).ToList();
            foreach (var kingdom in kingdomsToReanimate)
            {
                DeadRebelKingdoms.Remove(kingdom);
                var enemyKingdomList = FactionManager.GetEnemyKingdoms(kingdom).Where(k => !k.IsEliminated).ToList();
                if (enemyKingdomList.Count == 1)
                {
                    var parentKingdom = enemyKingdomList.First();
                    MakePeaceAction.Apply(kingdom, parentKingdom);
                    ConsolidateKingdomsAction.Apply(kingdom, parentKingdom);
                    DeadRebelKingdoms.Add(kingdom);
                }
            }
        }

        public static IReadOnlyDictionary<Kingdom, List<RebelFaction>> AllRebelFactions => Instance!.RebelFactions;
    }
}