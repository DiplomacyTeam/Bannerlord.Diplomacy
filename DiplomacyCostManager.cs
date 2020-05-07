using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes
{
    class DiplomacyCostManager
    {
        public static void deductInfluenceFromKingdom(Kingdom kingdom, float influenceCost)
        {
            kingdom.Leader.Clan.Influence -= influenceCost;
        }

        public static void deductInfluenceFromPlayerClan(float influenceCost)
        {
            Clan clan = Hero.MainHero.Clan;
            clan.Influence -= influenceCost;
        }

        public static void addInfluenceToPlayerClan(float addedInfluence)
        {
            Clan clan = Hero.MainHero.Clan;
            clan.Influence += addedInfluence;
        }

        public static void deductGoldFromPlayer(int goldCost)
        {
            Hero.MainHero.ChangeHeroGold(-goldCost);
        }

        public static void addGoldToPlayer(int addedGold)
        {
            Hero.MainHero.ChangeHeroGold(addedGold);
        }

        public static float getCurrentInfluenceForPlayerClan()
        {
            return Hero.MainHero.Clan.Influence;
        }

        public static void PayWarReparations(Kingdom kingdomMakingPeace, Kingdom otherKingdom)
        {
            PayWarReparations(kingdomMakingPeace, otherKingdom, DiplomacyCostCalculator.DetermineGoldCostForMakingPeace(kingdomMakingPeace, otherKingdom));
        }

        public static void PayWarReparations(Kingdom kingdom, Kingdom otherKingdom, int goldCost)
        {
            if (goldCost <= (kingdom.Leader.Gold / 2))
            {
                kingdom.Leader.ChangeHeroGold(-goldCost);
            }
            else
            {
                int totalKingdomWealth = DiplomacyCostCalculator.GetTotalKingdomWealth(kingdom);
                foreach (Hero hero in kingdom.Heroes)
                {
                    float proportionalWealth = hero.Gold / totalKingdomWealth;
                    int heroShare = (int)(proportionalWealth * goldCost);
                    hero.ChangeHeroGold(-heroShare);
                }
            }
            otherKingdom.Leader.ChangeHeroGold(goldCost);
        }
    }
}
