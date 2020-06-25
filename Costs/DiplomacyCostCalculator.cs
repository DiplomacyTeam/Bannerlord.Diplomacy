using DiplomacyFixes.Costs;
using DiplomacyFixes.DiplomaticAction.Alliance;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace DiplomacyFixes
{
    class DiplomacyCostCalculator
    {
        public static DiplomacyCost DetermineCostForDeclaringWar(Kingdom kingdom, bool forcePlayerCharacterCosts = false)
        {
            Clan clanPayingInfluence = forcePlayerCharacterCosts ? Clan.PlayerClan : kingdom.Leader.Clan;
            if (!Settings.Instance.EnableInfluenceCostsForDiplomacyActions)
                return new InfluenceCost(clanPayingInfluence, 0f);
            if (!Settings.Instance.ScalingInfluenceCosts)
                return new InfluenceCost(clanPayingInfluence, Settings.Instance.DeclareWarInfluenceCost);

            return new InfluenceCost(clanPayingInfluence, (float)Math.Floor(GetKingdomTierCount(kingdom) * Settings.Instance.ScalingInfluenceCostMultiplier));
        }

        public static DiplomacyCost DetermineCostForMakingPeace(Kingdom kingdom, bool forcePlayerCharacterCosts = false)
        {
            Clan clanPayingInfluence = forcePlayerCharacterCosts ? Clan.PlayerClan : kingdom.Leader.Clan;
            if (!Settings.Instance.EnableInfluenceCostsForDiplomacyActions)
                return new InfluenceCost(clanPayingInfluence, 0f);
            if (!Settings.Instance.ScalingInfluenceCosts)
                return new InfluenceCost(clanPayingInfluence, Settings.Instance.MakePeaceInfluenceCost);

            return new InfluenceCost(clanPayingInfluence, GetKingdomScalingFactor(kingdom));
        }

        private static float GetKingdomScalingFactor(Kingdom kingdom)
        {
            return (float)Math.Floor(GetKingdomTierCount(kingdom) * Settings.Instance.ScalingInfluenceCostMultiplier);
        }

        public static HybridCost DetermineCostForFormingNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            return new HybridCost(
                DetermineInfluenceCostForFormingNonAggressionPact(kingdom, otherKingdom, forcePlayerCharacterCosts),
                DetermineGoldCostForFormingNonAggressionPact(kingdom, otherKingdom, forcePlayerCharacterCosts));
        }

        public static InfluenceCost DetermineInfluenceCostForFormingNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            Clan clanPayingInfluence = forcePlayerCharacterCosts ? Clan.PlayerClan : kingdom.Leader.Clan;
            if (!Settings.Instance.EnableInfluenceCostsForDiplomacyActions)
                return new InfluenceCost(clanPayingInfluence, 0f);
            if (!Settings.Instance.ScalingInfluenceCosts)
                return new InfluenceCost(clanPayingInfluence, Settings.Instance.MakePeaceInfluenceCost);

            return new InfluenceCost(clanPayingInfluence, GetKingdomScalingFactor(kingdom));
        }

        private static GoldCost DetermineGoldCostForFormingNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            Hero giver = forcePlayerCharacterCosts ? Hero.MainHero : kingdom.Leader;

            float otherKingdomWarLoad = GetKingdomWarLoad(otherKingdom) + 1;

            int baseGoldCost = 500;
            int goldCostFactor = 100;
            int goldCost = (int)((MBMath.ClampFloat((1 / otherKingdomWarLoad), 0f, 1f) * GetKingdomScalingFactor(kingdom) * goldCostFactor) + baseGoldCost);

            return new GoldCost(giver, otherKingdom.Leader, goldCost);
        }

        private static float GetKingdomWarLoad(Kingdom kingdom)
        {
            return FactionManager.GetEnemyFactions(kingdom)?.Select(x => x.TotalStrength).Aggregate(0f, (result, item) => result + item) / kingdom.TotalStrength ?? 0f;
        }

        public static DiplomacyCost DetermineCostForSendingMessenger()
        {
            if (!Settings.Instance.EnableInfluenceCostsForDiplomacyActions)
            {
                return new InfluenceCost(Clan.PlayerClan, 0f);
            }

            if (Clan.PlayerClan.MapFaction.IsKingdomFaction)
            {
                return new InfluenceCost(Clan.PlayerClan, DetermineInfluenceCostForSendingMessenger());
            }
            else
            {
                return new GoldCost(Hero.MainHero, null, Settings.Instance.SendMessengerGoldCost);
            }
        }

        public static float DetermineInfluenceCostForSendingMessenger()
        {
            if (!Settings.Instance.EnableInfluenceCostsForDiplomacyActions)
                return 0f;

            return Settings.Instance.SendMessengerInfluenceCost;
        }

        public static int DetermineGoldCostForMakingPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
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

            return Math.Min((int)(GetKingdomScalingFactor(kingdomMakingPeace) * warExhaustionMultiplier), kingdomMakingPeace.Leader.Gold / 2);
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

        internal static DiplomacyCost DetermineCostForFormingAlliance(Kingdom kingdom, Kingdom otherKingdom, bool isPlayerRequested = false)
        {
            const float baseInfluenceCost = 100f;
            if (isPlayerRequested)
            {
                return new InfluenceCost(Clan.PlayerClan, MBMath.ClampFloat((float)Math.Pow(AllianceScoringModel.FormAllianceScoreThreshold / Math.Max(AllianceScoringModel.GetFormAllianceScore(kingdom, otherKingdom), 1f), 4), 1f, 256f) * baseInfluenceCost);
            }
            else
            {
                return new InfluenceCost(kingdom.Leader.Clan, baseInfluenceCost);
            }
        }
    }
}
