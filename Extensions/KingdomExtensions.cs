
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.Extensions
{
    public static class KingdomExtensions
    {
        public static float GetExpansionism(this Kingdom kingdom)
        {
            return ExpansionismManager.Instance.GetExpansionism(kingdom);
        }

        public static float GetExpansionismDiplomaticPenalty(this Kingdom kingdom)
        {
            return GetExpansionism(kingdom) - 50;
        }

        public static float GetMinimumExpansionism(this Kingdom kingdom)
        {
            return ExpansionismManager.Instance.GetMinimumExpansionism(kingdom); 
        }

        public static bool IsAlliedWith(this IFaction faction1, IFaction faction2)
        {
            if (faction1 == null || faction2 == null || faction1 == faction2)
            {
                return false;
            }
            StanceLink stanceLink = faction1.GetStanceWith(faction2);
            return stanceLink.IsAllied;
        }

        public static bool IsStrong(this Kingdom kingdom)
        {
            float medianStrength = GetMedianStrength();
            return kingdom.TotalStrength > medianStrength;
        }

        private static float GetMedianStrength()
        {
            float medianStrength;
            float[] kingdomStrengths = Kingdom.All.Select(curKingdom => curKingdom.TotalStrength).OrderBy(a => a).ToArray();

            int halfIndex = kingdomStrengths.Count() / 2;

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
    }
}
