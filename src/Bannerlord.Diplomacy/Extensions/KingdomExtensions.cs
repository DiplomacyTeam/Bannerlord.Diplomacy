using Diplomacy.CivilWar;
using Diplomacy.CivilWar.Factions;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Diplomacy.Extensions
{
    public static class KingdomExtensions
    {
        public static MBReadOnlyList<Kingdom> AllActiveKingdoms => new(Kingdom.All.Where(k => !k.IsEliminated).ToList());

        public static float GetExpansionism(this Kingdom kingdom) => ExpansionismManager.Instance!.GetExpansionism(kingdom);

        public static float GetExpansionismDiplomaticPenalty(this Kingdom kingdom) => Math.Min(-(GetExpansionism(kingdom) - 50), 0f);

        public static float GetMinimumExpansionism(this Kingdom kingdom) => ExpansionismManager.Instance!.GetMinimumExpansionism(kingdom);

        public static bool IsStrong(this Kingdom kingdom) => kingdom.CurrentTotalStrength > GetMedianStrength();

        private static float GetMedianStrength()
        {
            float medianStrength;
            var kingdomStrengths = AllActiveKingdoms.Select(curKingdom => curKingdom.CurrentTotalStrength).OrderBy(a => a).ToArray();

            var halfIndex = kingdomStrengths.Length / 2;

            if ((kingdomStrengths.Length % 2) == 0)
            {
                medianStrength = (kingdomStrengths.ElementAt(halfIndex) + kingdomStrengths.ElementAt(halfIndex - 1)) / 2;
            }
            else
            {
                medianStrength = kingdomStrengths.ElementAt(halfIndex);
            }
            return medianStrength;
        }

        public static bool IsRebelKingdom(this Kingdom kingdom) =>
            RebelFactionManager.AllRebelFactions.Values.SelectMany(x => x).Any(rf => kingdom == rf.RebelKingdom) || RebelFactionManager.Instance!.DeadRebelKingdoms.Contains(kingdom);

        public static bool IsRebelKingdomOf(this Kingdom kingdom, Kingdom parentKingdom) =>
            RebelFactionManager.AllRebelFactions.Values.SelectMany(x => x).Any(rf => kingdom == rf.RebelKingdom && parentKingdom == rf.ParentKingdom);

        public static bool HasRebellion(this Kingdom kingdom) => GetRebelFactions(kingdom).Any(x => x.AtWar);

        public static bool WillBeConsolidatedWith(this Kingdom kingdom, Kingdom otherKingdom, Kingdom losingKingdom)
        {
            if (kingdom.IsRebelKingdomOf(otherKingdom))
            {
                return ShouldConsolidate(kingdom, otherKingdom, losingKingdom);
            }
            else if (otherKingdom.IsRebelKingdomOf(kingdom))
            {
                return ShouldConsolidate(otherKingdom, kingdom, losingKingdom);
            }
            else
            {
                return false;
            }

            static bool ShouldConsolidate(Kingdom rebelKingdom, Kingdom parentKingdom, Kingdom losingKingdom)
            {
                return rebelKingdom == losingKingdom || parentKingdom.GetRebelFactions().Any(x => x.RebelKingdom == rebelKingdom && x.ConsolidateOnSuccess);
            }
        }

        public static IEnumerable<RebelFaction> GetRebelFactions(this Kingdom kingdom) => RebelFactionManager.GetRebelFaction(kingdom);

        public static IEnumerable<Kingdom> GetEnemyKingdoms(this Kingdom kingdom) => AllActiveKingdoms.Where(x => x.IsAtWarWith(kingdom) || x.IsAtConstantWarWith(kingdom));
    }
}