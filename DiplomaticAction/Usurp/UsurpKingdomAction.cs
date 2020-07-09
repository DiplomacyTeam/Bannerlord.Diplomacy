using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace DiplomacyFixes.DiplomaticAction.Usurp
{
    public class UsurpKingdomAction
    {
        public static void Apply(Clan usurpingClan)
        {
            List<Clan> supportingClans, opposingClans;
            GetClanSupport(usurpingClan, out supportingClans, out opposingClans);

            usurpingClan.Influence -= usurpingClan.Kingdom.RulingClan.Influence;
            usurpingClan.Kingdom.RulingClan.Influence = 0;
            usurpingClan.Kingdom.RulingClan = usurpingClan;

            AdjustRelations(usurpingClan, supportingClans, 10);
            AdjustRelations(usurpingClan, opposingClans, 20);
        }

        private static void AdjustRelations(Clan usurpingClan, List<Clan> clans, int baseValue)
        {
            Hero leader = usurpingClan.Leader;
            foreach (Clan clan in clans)
            {
                if (usurpingClan == clan)
                {
                    continue;
                }

                Hero otherLeader = clan.Leader;
                int honor = otherLeader.GetHeroTraits().Honor;
                int calculating = otherLeader.GetHeroTraits().Calculating;
                int value = honor >= calculating ? baseValue : default;
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(leader, otherLeader, -value);
            }
        }

        public static bool CanUsurp(Clan usurpingClan)
        {
            if (!usurpingClan.MapFaction.IsKingdomFaction || usurpingClan.Kingdom.RulingClan == usurpingClan)
            {
                return false;
            }

            Clan rulingClan = usurpingClan.Kingdom.RulingClan;
            GetClanSupport(usurpingClan, out int supportingClanTiers, out int opposingClanTiers);
            return usurpingClan.Influence > GetUsurpInfluenceCost(usurpingClan) && supportingClanTiers > opposingClanTiers;
        }

        public static void GetClanSupport(Clan usurpingClan, out int supportingClanTiers, out int opposingClanTiers)
        {
            Clan rulingClan = usurpingClan.Kingdom.RulingClan;
            List<Clan> supportingClans, opposingClans;
            GetClanSupport(usurpingClan, out supportingClans, out opposingClans);

            supportingClanTiers = supportingClans.Select(clan => clan.Tier).Sum() + usurpingClan.Tier;
            opposingClanTiers = opposingClans.Select(clan => clan.Tier).Sum() + rulingClan.Tier;
        }

        public static float GetUsurpInfluenceCost(Clan usurpingClan)
        {
            Clan rulingClan = usurpingClan.Kingdom.RulingClan;
            return rulingClan.Influence;
        }

        private static void GetClanSupport(Clan usurpingClan, out List<Clan> supportingClans, out List<Clan> opposingClans)
        {
            Kingdom kingdom = usurpingClan.Kingdom;
            Clan rulingClan = usurpingClan.Kingdom.RulingClan;

            IEnumerable<Clan> validClans = kingdom.Clans.Except(new Clan[] { usurpingClan, rulingClan });

            supportingClans = validClans.Where(clan => usurpingClan.Leader.GetRelation(clan.Leader) > rulingClan.Leader.GetRelation(clan.Leader)).ToList();
            opposingClans = validClans.Except(supportingClans).ToList();
        }
    }
}
