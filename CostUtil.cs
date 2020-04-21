using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes
{
    class CostUtil
    {
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
    }
}
