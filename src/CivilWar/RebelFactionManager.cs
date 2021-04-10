using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace Diplomacy.CivilWar
{
    class RebelFactionManager
    {
        public static RebelFactionManager? Instance { get; private set; }

        [SaveableProperty(1)]
        private Dictionary<Kingdom, List<RebelFaction>> RebelFactions { get; set; }
        [SaveableProperty(2)]
        public List<Kingdom> DeadRebelKingdoms { get; private set; }
        [SaveableProperty(3)]
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

        public static void RegisterRebelFaction(Kingdom kingdom, RebelFaction rebelFaction)
        {
            if (!CanStartRebelFaction(kingdom))
            {
                return;
            }

            if (Instance!.RebelFactions.TryGetValue(kingdom, out List<RebelFaction> rebelFactions))
            {
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

        public static bool CanStartRebelFaction(Kingdom kingdom)
        {
            if (Instance!.RebelFactions.TryGetValue(kingdom, out List<RebelFaction> rebelFactions))
            {
                if (rebelFactions.Count >= 3 || rebelFactions.Where(x => x.AtWar).Any())
                    return false;
            }

            if (Instance!.LastCivilWar.TryGetValue(kingdom, out CampaignTime lastTime))
            {
                float daysSinceLastCivilWar = lastTime.ElapsedDaysUntilNow;

                if (daysSinceLastCivilWar < Settings.Instance!.MinimumTimeSinceLastCivilWarInDays)
                    return false;
            }

            return true;
        }

        public static bool HasRebelFaction(Kingdom kingdom)
        {
            return Instance!.RebelFactions.ContainsKey(kingdom);
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

        public static IReadOnlyDictionary<Kingdom, List<RebelFaction>> AllRebelFactions { get => Instance!.RebelFactions; }
    }
}