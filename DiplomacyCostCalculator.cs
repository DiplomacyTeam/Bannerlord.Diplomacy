using DiplomacyFixes.Alliance;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace DiplomacyFixes
{
    class DiplomacyCostCalculator
    {
        public static float DetermineInfluenceCostForDeclaringWar(Kingdom kingdom)
        {
            if (!Settings.Instance.EnableInfluenceCostsForDiplomacyActions)
                return 0f;
            if (!Settings.Instance.ScalingInfluenceCosts)
                return Settings.Instance.DeclareWarInfluenceCost;

            return (float)Math.Floor(GetKingdomTierCount(kingdom) * Settings.Instance.ScalingInfluenceCostMultiplier);
        }

        public static float DetermineInfluenceCostForMakingPeace(Kingdom kingdom)
        {
            if (!Settings.Instance.EnableInfluenceCostsForDiplomacyActions)
                return 0f;
            if (!Settings.Instance.ScalingInfluenceCosts)
                return Settings.Instance.MakePeaceInfluenceCost;

            return (float)Math.Floor(GetKingdomTierCount(kingdom) * Settings.Instance.ScalingInfluenceCostMultiplier);
        }

        public static float DetermineInfluenceCostForSendingMessenger()
        {
            if (!Settings.Instance.EnableInfluenceCostsForDiplomacyActions)
                return 0f;

            return Settings.Instance.SendMessengerInfluenceCost;
        }

        public static int DetermineGoldCostForMakingPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom)
        {
            if (!Settings.Instance.EnableInfluenceCostsForDiplomacyActions)
            {
                return 0;
            }

            float warExhaustionMultiplier = 1f;
            if (Settings.Instance.EnableWarExhaustion)
            {
                float kingdomMakingPeaceWarExhaustion = WarExhaustionManager.Instance.GetWarExhaustion(kingdomMakingPeace, otherKingdom);
                float otherKingdomWarExhaustion = WarExhaustionManager.Instance.GetWarExhaustion(otherKingdom, kingdomMakingPeace);
                float relativeWarExhaustion = (kingdomMakingPeaceWarExhaustion + 1f) / (otherKingdomWarExhaustion + 1f) - 1f; ;
                warExhaustionMultiplier = MBMath.ClampFloat(relativeWarExhaustion, 0, (((WarExhaustionManager.DefaultMaxWarExhaustion / Settings.Instance.MaxWarExhaustion) / 20) * kingdomMakingPeaceWarExhaustion) - 1f);
            }

            return Math.Min((int)(GetKingdomTierCount(kingdomMakingPeace) * Settings.Instance.ScalingWarReparationsGoldCostMultiplier * warExhaustionMultiplier), kingdomMakingPeace.Leader.Gold / 2);
        }

        public static int GetTotalKingdomWealth(Kingdom kingdom)
        {
            int payableWealth = 0;
            foreach (Hero hero in kingdom.Heroes)
            {
                payableWealth += hero.Gold;
            }
            return payableWealth;
        }

        private static int GetKingdomTierCount(Kingdom kingdom)
        {
            int tierTotal = 0;
            foreach (Clan clan in kingdom.Clans)
            {
                tierTotal += clan.Tier;
            }
            return tierTotal;
        }

        internal static float DetermineInfluenceCostForFormingAlliance(Kingdom kingdom, Kingdom otherKingdom, bool isPlayerRequested = false)
        {
            const float baseInfluenceCost = 100f;
            if (isPlayerRequested)
            {
                return MBMath.ClampFloat((float)Math.Pow(AllianceScoringModel.FormAllianceScoreThreshold / Math.Max(AllianceScoringModel.GetFormAllianceScore(kingdom, otherKingdom), 1f), 4), 1f, 256f) * baseInfluenceCost;
            }
            else
            {
                return baseInfluenceCost;
            }
        }
    }
}
