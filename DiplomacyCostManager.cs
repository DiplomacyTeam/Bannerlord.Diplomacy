using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace DiplomacyFixes
{
    class DiplomacyCostManager
    {
        public static void DeductInfluenceFromKingdom(Kingdom kingdom, float influenceCost)
        {
            kingdom.Leader.Clan.Influence = MBMath.ClampFloat(kingdom.Leader.Clan.Influence - influenceCost, 0f, float.MaxValue);
        }

        public static void deductInfluenceFromPlayerClan(float influenceCost)
        {
            Clan clan = Hero.MainHero.Clan;
            clan.Influence = MBMath.ClampFloat(clan.Influence - influenceCost, 0f, float.MaxValue);
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
            if (goldCost >= kingdom.Leader.Gold)
            {
                GiveGoldAction.ApplyBetweenCharacters(kingdom.Leader, otherKingdom.Leader, kingdom.Leader.Gold);
            }
            else
            {
                GiveGoldAction.ApplyBetweenCharacters(kingdom.Leader, otherKingdom.Leader, goldCost);
            }
        }
    }
}
