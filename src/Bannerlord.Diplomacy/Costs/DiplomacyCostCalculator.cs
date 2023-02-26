using Diplomacy.CivilWar.Factions;
using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.Extensions;
using Diplomacy.WarExhaustion;

using System;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Diplomacy.Costs
{
    class DiplomacyCostCalculator
    {
        private static float AllianceFactor => 5.0f;
        private static float PeaceFactor => 20.0f;
        private static float WarFactor => 10.0f;

        public static InfluenceCost DetermineCostForDeclaringWar(Kingdom kingdom, bool forcePlayerCharacterCosts = false)
        {
            var clanPayingInfluence = forcePlayerCharacterCosts ? Clan.PlayerClan : kingdom.Leader.Clan;
            if (!Settings.Instance!.EnableInfluenceCostsForDiplomacyActions)
                return new InfluenceCost(clanPayingInfluence, 0f);
            if (!Settings.Instance!.ScalingInfluenceCosts)
                return new InfluenceCost(clanPayingInfluence, Settings.Instance!.DeclareWarInfluenceCost);

            return new InfluenceCost(clanPayingInfluence, GetKingdomScalingFactorForInfluence(kingdom) * WarFactor);
        }

        public static HybridCost DetermineCostForMakingPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            return new(
                DetermineInfluenceCostForMakingPeace(kingdomMakingPeace, otherKingdom, forcePlayerCharacterCosts),
                DetermineGoldCostForMakingPeace(kingdomMakingPeace, otherKingdom, forcePlayerCharacterCosts),
                DetermineReparationsForMakingPeace(kingdomMakingPeace, otherKingdom, forcePlayerCharacterCosts));
        }

        internal static InfluenceCost DetermineInfluenceCostForMakingPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool forcePlayerCharacterCosts)
        {
            var clanPayingInfluence = forcePlayerCharacterCosts ? Clan.PlayerClan : kingdomMakingPeace.Leader.Clan;
            if (!Settings.Instance!.EnableInfluenceCostsForDiplomacyActions)
                return new InfluenceCost(clanPayingInfluence, 0f);

            //Making peace is free for critically exhausted factions that not yet lost
            if (Settings.Instance!.EnableWarExhaustion && WarExhaustionManager.Instance!.HasCriticalWarExhaustion(kingdomMakingPeace, otherKingdom, checkMaxWarExhaustion: true))
                return new InfluenceCost(clanPayingInfluence, 0f);

            if (!Settings.Instance!.ScalingInfluenceCosts)
                return new InfluenceCost(clanPayingInfluence, Settings.Instance!.MakePeaceInfluenceCost);

            return new InfluenceCost(clanPayingInfluence, GetKingdomScalingFactorForInfluence(kingdomMakingPeace) * PeaceFactor);
        }

        internal static GoldCost DetermineGoldCostForMakingPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            var giver = forcePlayerCharacterCosts ? Hero.MainHero : kingdomMakingPeace.Leader;

            var baseGoldCost = 500;
            int goldCost = Math.Min(baseGoldCost, kingdomMakingPeace.Leader.Gold);
            goldCost = 10 * (goldCost / 10);

            //This is a cost of organization process and thus has no addressee
            return new GoldCost(giver, goldCost);
        }

        private static KingdomWalletCost DetermineReparationsForMakingPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool forcePlayerCharacterCosts)
        {
            var giver = kingdomMakingPeace;
            var receiver = otherKingdom;
            var reparationsCost = 0;

            if (!Settings.Instance!.EnableWarExhaustion)
                return new(giver, receiver, reparationsCost);

            //No reparations between kingdoms that are about to be destroyed
            var shouldBeDestroyed = KingdomPeaceAction.ShouldKingdomsBeDestroyed(kingdomMakingPeace, otherKingdom);
            if (shouldBeDestroyed.KingdomMakingPeace || shouldBeDestroyed.OtherKingdom)
                return new(giver, receiver, reparationsCost);
            if (kingdomMakingPeace.IsRebelKingdomOf(otherKingdom) || (otherKingdom.IsRebelKingdomOf(kingdomMakingPeace) && kingdomMakingPeace.GetRebelFactions().Any(x => x.RebelKingdom == otherKingdom && x is not SecessionFaction)))
                return new(giver, receiver, reparationsCost);

            var kingdomMakingPeaceWarExhaustion = WarExhaustionManager.Instance!.GetWarExhaustion(kingdomMakingPeace, otherKingdom);
            var otherKingdomWarExhaustion = WarExhaustionManager.Instance!.GetWarExhaustion(otherKingdom, kingdomMakingPeace);

            if (kingdomMakingPeaceWarExhaustion > otherKingdomWarExhaustion && WarExhaustionManager.IsCriticalWarExhaustion(kingdomMakingPeaceWarExhaustion))
            {
                reparationsCost = CalculateReparations(kingdomMakingPeace, otherKingdom, kingdomMakingPeaceWarExhaustion, otherKingdomWarExhaustion);
            }
            else if (otherKingdomWarExhaustion > kingdomMakingPeaceWarExhaustion && WarExhaustionManager.IsCriticalWarExhaustion(otherKingdomWarExhaustion))
            {
                GetReparationsFromOtherKingdom(kingdomMakingPeace, otherKingdom, out giver, out receiver, out reparationsCost, kingdomMakingPeaceWarExhaustion, otherKingdomWarExhaustion);
            }
            else if (kingdomMakingPeaceWarExhaustion == otherKingdomWarExhaustion)
            {
                var otherKingdomLost = WarExhaustionManager.Instance!.GetWarResult(otherKingdom, kingdomMakingPeace) == WarExhaustionManager.WarResult.Loss;
                if (otherKingdomLost)
                {
                    GetReparationsFromOtherKingdom(kingdomMakingPeace, otherKingdom, out giver, out receiver, out reparationsCost, kingdomMakingPeaceWarExhaustion, otherKingdomWarExhaustion, otherKingdomLost);
                }
                else
                {
                    var kingdomMakingPeaceLost = WarExhaustionManager.Instance!.GetWarResult(kingdomMakingPeace, otherKingdom) == WarExhaustionManager.WarResult.Loss;
                    if (kingdomMakingPeaceLost)
                        reparationsCost = CalculateReparations(kingdomMakingPeace, otherKingdom, kingdomMakingPeaceWarExhaustion, otherKingdomWarExhaustion, kingdomMakingPeaceLost);
                }
            }
            reparationsCost = 100 * (reparationsCost / 100);

            return new(giver, receiver, reparationsCost);

            static int CalculateReparations(Kingdom kingdomPayingReparations, Kingdom otherKingdom, float payingKingdomWarExhaustion, float otherKingdomWarExhaustion, bool? payerLostWar = null)
            {
                var lossReparations = (payerLostWar ?? WarExhaustionManager.Instance!.GetWarResult(kingdomPayingReparations, otherKingdom) == WarExhaustionManager.WarResult.Loss) ? Settings.Instance!.DefeatedGoldCost : 0;
                var warExhaustionReparations = otherKingdomWarExhaustion * ((payingKingdomWarExhaustion - otherKingdomWarExhaustion) / (WarExhaustionManager.IsCriticalWarExhaustion(otherKingdomWarExhaustion) ? 10f : 5f));
                return (int) ((lossReparations + warExhaustionReparations) * GetKingdomScalingFactorForReparations(kingdomPayingReparations));
            }

            static void GetReparationsFromOtherKingdom(Kingdom kingdomMakingPeace, Kingdom otherKingdom, out Kingdom giver, out Kingdom receiver, out int reparationsCost, float kingdomMakingPeaceWarExhaustion, float otherKingdomWarExhaustion, bool? otherKingdomLost = null)
            {
                giver = otherKingdom;
                receiver = kingdomMakingPeace;
                reparationsCost = CalculateReparations(otherKingdom, kingdomMakingPeace, otherKingdomWarExhaustion, kingdomMakingPeaceWarExhaustion, otherKingdomLost);
            }
        }

        private static float GetKingdomScalingFactorForInfluence(Kingdom kingdom) => (float) Math.Floor(GetKingdomTierCount(kingdom) * Settings.Instance!.ScalingInfluenceCostMultiplier);

        private static float GetKingdomScalingFactorForGold(Kingdom kingdom) => Settings.Instance!.ScalingGoldCosts ? (float) Math.Floor(GetKingdomTierCount(kingdom) * Settings.Instance!.ScalingGoldCostMultiplier) : 100f;

        private static float GetKingdomScalingFactorForReparations(Kingdom kingdom) => Settings.Instance!.ScalingGoldCosts ? (float) Math.Floor(GetKingdomTierCount(kingdom) * Settings.Instance!.ScalingWarReparationsGoldCostMultiplier) : 1000f;

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

            return new InfluenceCost(clanPayingInfluence, GetKingdomScalingFactorForInfluence(kingdom));
        }

        private static GoldCost DetermineGoldCostForFormingNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            var giver = forcePlayerCharacterCosts ? Hero.MainHero : kingdom.Leader;

            var otherKingdomWarLoad = GetKingdomWarLoad(otherKingdom) + 1;

            var baseGoldCost = 500;
            var goldCostFactor = 100;
            var goldCost = (int) ((MBMath.ClampFloat((1 / otherKingdomWarLoad), 0f, 1f) * GetKingdomScalingFactorForGold(kingdom) * goldCostFactor) + baseGoldCost);

            //This is a cost of organization process and thus has no addressee
            return new GoldCost(giver, goldCost);
        }

        private static float GetKingdomWarLoad(Kingdom kingdom)
        {
            return FactionManager.GetEnemyFactions(kingdom)?.Select(x => x.TotalStrength).Aggregate(0f, (result, item) => result + item) / kingdom.TotalStrength ?? 0f;
        }

        public static GoldCost DetermineCostForSendingMessenger()
        {
            return new(Hero.MainHero, Settings.Instance!.SendMessengerGoldCost);
        }

        private static int GetKingdomTierCount(Kingdom kingdom)
        {
            var tierTotal = 0;
            foreach (var clan in kingdom.Clans)
            {
                tierTotal += clan.IsUnderMercenaryService ? clan.Tier / 3 : clan.Tier;
            }
            return MBMath.ClampInt(tierTotal, 5, 50);
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

            return new InfluenceCost(clanPayingInfluence, GetKingdomScalingFactorForInfluence(kingdom) * AllianceFactor);
        }

        private static GoldCost DetermineGoldCostForFormingAlliance(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            var giver = forcePlayerCharacterCosts ? Hero.MainHero : kingdom.Leader;

            var otherKingdomWarLoad = GetKingdomWarLoad(otherKingdom) + 1;

            var baseGoldCost = 500;
            var goldCostFactor = 100;
            var goldCost = (int) ((MBMath.ClampFloat((1 / otherKingdomWarLoad), 0f, 1f) * GetKingdomScalingFactorForGold(kingdom) * AllianceFactor * goldCostFactor) + baseGoldCost);

            //This is a cost of organization process and thus has no addressee
            return new GoldCost(giver, goldCost);
        }
    }
}