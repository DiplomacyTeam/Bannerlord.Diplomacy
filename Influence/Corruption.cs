using System.Linq;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.Influence
{
    public static class CorruptionExtensions
    {
        public static float GetCorruption(this Clan clan)
        {
            int numFiefsTooMany = clan.Fiefs.Count() - clan.Tier;
            if (numFiefsTooMany > 0)
            {
                int factor = numFiefsTooMany > 5 ? 2 : 1;
                return numFiefsTooMany * factor;
            }
            else
            {
                return 0f;
            }
        }

        public static bool HasMaximumFiefs(this Clan clan)
        {
            int numFiefsTooMany = clan.Fiefs.Count() - clan.Tier;
            return numFiefsTooMany >= 0;
        } 
    }
}
