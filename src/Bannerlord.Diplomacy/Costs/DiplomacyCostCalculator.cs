using System;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Diplomacy.Costs
{
    class DiplomacyCostCalculator
    {
        private static float AllianceFactor => 5.0f;
        private static float PeaceFactor => 2.0f;

        public static DiplomacyCost DetermineCostForDeclaringWar(Kingdom kingdom, bool forcePlayerCharacterCosts = false)
        {
            var clanPayingInfluence = forcePlayerCharacterCosts ? Clan.PlayerClan : kingdom.Leader.Clan;
            if (!Settings.Instance!.EnableInfluenceCostsForDiplomacyActions)
                return new InfluenceCost(clanPayingInfluence, 0f);
            if (!Settings.Instance!.ScalingInfluenceCosts)
                return new InfluenceCost(clanPayingInfluence, Settings.Instance!.DeclareWarInfluenceCost);

            return new InfluenceCost(clanPayingInfluence, (float) Math.Floor(GetKingdomTierCount(kingdom) * Settings.Instance!.ScalingInfluenceCostMultiplier));
        }

        public static HybridCost DetermineCostForMakingPeace(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            return new(
                DetermineInfluenceCostForMakingPeace(kingdom, forcePlayerCharacterCosts),
                DetermineGoldCostForMakingPeace(kingdom, otherKingdom, forcePlayerCharacterCosts));
        }

        private static InfluenceCost DetermineInfluenceCostForMakingPeace(Kingdom kingdom, bool forcePlayerCharacterCosts)
        {
            var clanPayingInfluence = forcePlayerCharacterCosts ? Clan.PlayerClan : kingdom.Leader.Clan;
            InfluenceCost influenceCost;
            if (!Settings.Instance!.EnableInfluenceCostsForDiplomacyActions)
            {
                influenceCost = new InfluenceCost(clanPayingInfluence, 0f);
            }
            else if (!Settings.Instance!.ScalingInfluenceCosts)
            {
                influenceCost = new InfluenceCost(clanPayingInfluence, Settings.Instance!.MakePeaceInfluenceCost);
            }
            else
            {
                influenceCost = new InfluenceCost(clanPayingInfluence, GetKingdomScalingFactor(kingdom) * PeaceFactor);
            }

            return influenceCost;
        }

        private static float GetKingdomScalingFactor(Kingdom kingdom)
        {
            return (float) Math.Floor(GetKingdomTierCount(kingdom) * Settings.Instance!.ScalingInfluenceCostMultiplier);
        }

        public static HybridCost DetermineCostForFormingNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            return new(
                DetermineInfluenceCostForFormingNonAggressionPact(kingdom, otherKingdom, forcePlayerCharacterCosts),
                DetermineGoldCostForFormingNonAggressionPact(kingdom, otherKingdom, forcePlayerCharacterCosts));
        }

        public static InfluenceCost DetermineInfluenceCostForFormingNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            var clanPayingInfluence = forcePlayerCharacterCosts ? Clan.PlayerClan : kingdom.Leader.Clan;
            if (!Settings.Instance!.EnableInfluenceCostsForDiplomacyActions)
                return new InfluenceCost(clanPayingInfluence, 0f);
            if (!Settings.Instance!.ScalingInfluenceCosts)
                return new InfluenceCost(clanPayingInfluence, Settings.Instance!.MakePeaceInfluenceCost);

            return new InfluenceCost(clanPayingInfluence, GetKingdomScalingFactor(kingdom));
        }

        private static GoldCost DetermineGoldCostForFormingNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            var giver = forcePlayerCharacterCosts ? Hero.MainHero : kingdom.Leader;

            var otherKingdomWarLoad = GetKingdomWarLoad(otherKingdom) + 1;

            var baseGoldCost = 500;
            var goldCostFactor = 100;
            var goldCost = (int) ((MBMath.ClampFloat((1 / otherKingdomWarLoad), 0f, 1f) * GetKingdomScalingFactor(kingdom) * goldCostFactor) + baseGoldCost);

            return new GoldCost(giver, otherKingdom.Leader, goldCost);
        }

        private static float GetKingdomWarLoad(Kingdom kingdom)
        {
            return FactionManager.GetEnemyFactions(kingdom)?.Select(x => x.TotalStrength).Aggregate(0f, (result, item) => result + item) / kingdom.TotalStrength ?? 0f;
        }

        public static DiplomacyCost DetermineCostForSendingMessenger()
        {
            return new GoldCost(Hero.MainHero, null, Settings.Instance!.SendMessengerGoldCost);
        }

        private static GoldCost DetermineGoldCostForMakingPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            var giver = forcePlayerCharacterCosts ? Hero.MainHero : kingdomMakingPeace.Leader;

            int goldCost;
            if (!Settings.Instance!.EnableInfluenceCostsForDiplomacyActions)
            {
                goldCost = 0;
            }
            else
            {
                var warExhaustionMultiplier = 1f;
                if (Settings.Instance!.EnableWarExhaustion)
                {
                    var kingdomMakingPeaceWarExhaustion = WarExhaustionManager.Instance.GetWarExhaustion(kingdomMakingPeace, otherKingdom);
                    var otherKingdomWarExhaustion = WarExhaustionManager.Instance.GetWarExhaustion(otherKingdom, kingdomMakingPeace);
                    var relativeWarExhaustion = (kingdomMakingPeaceWarExhaustion + 1f) / (otherKingdomWarExhaustion + 1f);
                    warExhaustionMultiplier = MBMath.ClampFloat(relativeWarExhaustion, 0, ((1f / 20f) * kingdomMakingPeaceWarExhaustion) + 1f);
                }
                var baseGoldCost = 500;
                goldCost = Math.Min((int) ((GetKingdomScalingFactor(kingdomMakingPeace) * Settings.Instance!.ScalingWarReparationsGoldCostMultiplier) * warExhaustionMultiplier) + baseGoldCost, kingdomMakingPeace.Leader.Gold / 2);
            }
            return new GoldCost(giver, otherKingdom.Leader, goldCost);
        }

        private static int GetKingdomTierCount(Kingdom kingdom)
        {
            var tierTotal = 0;
            foreach (var clan in kingdom.Clans)
            {
                tierTotal += clan.Tier;
            }
            return Math.Min(tierTotal, 20);
        }

        public static HybridCost DetermineCostForFormingAlliance(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            return new(
                DetermineInfluenceCostForFormingAlliance(kingdom, forcePlayerCharacterCosts),
                DetermineGoldCostForFormingAlliance(kingdom, otherKingdom, forcePlayerCharacterCosts));
        }

        private static InfluenceCost DetermineInfluenceCostForFormingAlliance(Kingdom kingdom, bool forcePlayerCharacterCosts = false)
        {
            var clanPayingInfluence = forcePlayerCharacterCosts ? Clan.PlayerClan : kingdom.Leader.Clan;
            if (!Settings.Instance!.EnableInfluenceCostsForDiplomacyActions)
                return new InfluenceCost(clanPayingInfluence, 0f);
            if (!Settings.Instance!.ScalingInfluenceCosts)
                return new InfluenceCost(clanPayingInfluence, Settings.Instance!.MakePeaceInfluenceCost * 2.0f);

            return new InfluenceCost(clanPayingInfluence, GetKingdomScalingFactor(kingdom) * AllianceFactor);
        }

        private static GoldCost DetermineGoldCostForFormingAlliance(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            var giver = forcePlayerCharacterCosts ? Hero.MainHero : kingdom.Leader;

            var otherKingdomWarLoad = GetKingdomWarLoad(otherKingdom) + 1;

            var baseGoldCost = 500;
            var goldCostFactor = 100;
            var goldCost = (int) ((MBMath.ClampFloat((1 / otherKingdomWarLoad), 0f, 1f) * GetKingdomScalingFactor(kingdom) * AllianceFactor * goldCostFactor) + baseGoldCost);

            return new GoldCost(giver, otherKingdom.Leader, goldCost);
        }
    }
}