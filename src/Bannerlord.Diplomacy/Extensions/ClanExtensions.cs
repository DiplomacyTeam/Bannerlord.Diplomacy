using Diplomacy.CivilWar;
using Diplomacy.CivilWar.Factions;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace Diplomacy.Extensions
{
    internal static class ClanExtensions
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

        public static IEnumerable<Town> GetPermanentFiefs(this Clan clan) => clan.Fiefs.Where(fief => !fief.IsOwnerUnassigned);

        public static IEnumerable<RebelFaction> GetRebelFactions(this Clan clan)
        {
            if (clan.Kingdom is null)
            {
                yield break;
            }

            foreach (var faction in RebelFactionManager.GetRebelFaction(clan.Kingdom))
            {
                if (faction.Clans.Contains(clan))
                    yield return faction;
            }
        }

        public static RebelFaction? GetSponsoredRebelFaction(this Clan clan)
        {
            if (clan.Kingdom != null)
            {
                return RebelFactionManager.GetRebelFaction(clan.Kingdom).FirstOrDefault(x => x.SponsorClan == clan);
            }
            else
            {
                return default;
            }
        }
    }
}