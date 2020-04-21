using System;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes
{
    class CostCalculator
    {
        public static float determineInfluenceCostForDeclaringWar()
        {
            if (!Settings.Instance.EnableInfluenceCostsForDiplomacyActions)
                return 0f;
            if(!Settings.Instance.ScalingInfluenceCosts)
                return Settings.Instance.DeclareWarInfluenceCost;

            return (float)Math.Floor(getKingdomTierCount() * Settings.Instance.ScalingInfluenceCostMultiplier);
        }

        public static float determineInfluenceCostForMakingPeace()
        {
            if (!Settings.Instance.EnableInfluenceCostsForDiplomacyActions)
                return 0f;
            if (!Settings.Instance.ScalingInfluenceCosts)
                return Settings.Instance.MakePeaceInfluenceCost;

            return (float) Math.Floor(getKingdomTierCount() * Settings.Instance.ScalingInfluenceCostMultiplier);
        }

        private static int getKingdomTierCount()
        {
            Kingdom kingdom;
            if ((kingdom = (Hero.MainHero.MapFaction as Kingdom)) != null)
            {
                int tierTotal = 0;
                foreach (Clan clan in kingdom.Clans)
                {
                    tierTotal += clan.Tier;
                }
                return tierTotal;
            }
            return 0;
        }

    }
}
