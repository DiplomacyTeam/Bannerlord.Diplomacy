using System.Linq;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.Extensions
{
    public static class ClanExtensions
    {
        public static float GetCorruption(this Clan clan)
        {
            float corruption = 0f;
            int numFiefsTooMany = clan.Fiefs.Count() - clan.Tier;
            if (numFiefsTooMany > 0)
            {
                int factor = numFiefsTooMany > 5 ? 2 : 1;
                corruption = numFiefsTooMany * factor;
            }

            return corruption;
        }

        public static bool HasMaximumFiefs(this Clan clan)
        {
            int numFiefsTooMany = clan.Fiefs.Count() - clan.Tier;
            return numFiefsTooMany >= 0;
        }

        public static float GetExpansionism(this Clan clan)
        {
            return clan.MapFaction.IsKingdomFaction ? ExpansionismManager.Instance.GetExpansionism(clan.MapFaction) : 0f;
        }
    }
}
