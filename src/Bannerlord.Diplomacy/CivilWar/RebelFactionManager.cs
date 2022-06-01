﻿using Diplomacy.CivilWar.Factions;

using JetBrains.Annotations;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
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
            Kingdom kingdom = rebelFaction.ParentKingdom;

            if (Instance!.RebelFactions.TryGetValue(kingdom, out List<RebelFaction> rebelFactions))
            {
                // if we're starting a secession faction, remove this clan from other secession factions
                if (rebelFaction.RebelDemandType == RebelDemandType.Secession)
                {
                    var otherSecessionFactions = rebelFactions.Where(x => x.RebelDemandType == RebelDemandType.Secession && x.Clans.Contains(rebelFaction.SponsorClan));
                    foreach (RebelFaction faction in otherSecessionFactions)
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

            if (Instance!.RebelFactions.TryGetValue(kingdom, out List<RebelFaction> rebelFactions))
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

        public static IReadOnlyDictionary<Kingdom, List<RebelFaction>> AllRebelFactions => Instance!.RebelFactions;
    }
}