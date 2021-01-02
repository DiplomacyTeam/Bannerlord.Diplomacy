using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace Diplomacy.Extensions
{
    public static class ClanExtensions
    {
        public static float GetCorruption(this Clan clan)
        {
            var corruption = 0f;
            var numFiefsTooMany = clan.GetPermanentFiefs().Count() - clan.Tier;
            if (numFiefsTooMany > 0)
            {
                var factor = numFiefsTooMany > 5 ? 2 : 1;
                corruption = numFiefsTooMany * factor;
            }

            return corruption;
        }

        public static bool HasMaximumFiefs(this Clan clan)
        {
            var numFiefsTooMany = clan.GetPermanentFiefs().Count() - clan.Tier;
            return numFiefsTooMany >= 0;
        }

        public static IEnumerable<Town> GetPermanentFiefs(this Clan clan)
        {
            return clan.Fiefs.Where(fief => !fief.IsOwnerUnassigned);
        }
    }
}
