using Diplomacy.DiplomaticAction;

using System;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Diplomacy.Costs
{
    internal static class DiplomacyCostCalculator
    {
        //FIXME: Ideally, we could divide the costs in gold for some actions into direct and indirect.
        //Direct costs will go to the other party, while indirect costs will simply be written off, representing a cost of organization process and will be subject to the PartyTypeFactor.
        private static float AllianceFactor { get; } = 5.0f;
        private static float PeaceFactor { get; } = 2.0f;

        public static DiplomacyCost DetermineCostForDeclaringWar(Kingdom kingdom, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            var clanPayingInfluence = forcePlayerCharacterCosts ? Clan.PlayerClan : kingdom.Leader.Clan;
            if (!Settings.Instance!.EnableInfluenceCostsForDiplomacyActions)
                return new InfluenceCost(clanPayingInfluence, 0f);
            if (!Settings.Instance!.ScalingInfluenceCosts)
                return new InfluenceCost(clanPayingInfluence, Settings.Instance!.DeclareWarInfluenceCost * GetPartyTypeFactor(kingdomPartyType));

            return new InfluenceCost(clanPayingInfluence, (float)Math.Floor(GetKingdomTierCount(kingdom) * Settings.Instance!.ScalingInfluenceCostMultiplier * GetPartyTypeFactor(kingdomPartyType)));
        }

        public static HybridCost DetermineCostForMakingPeace(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            return new(
                DetermineInfluenceCostForMakingPeace(kingdom, forcePlayerCharacterCosts, kingdomPartyType),
                DetermineGoldCostForMakingPeace(kingdom, otherKingdom, forcePlayerCharacterCosts, kingdomPartyType));
        }

        public static InfluenceCost DetermineInfluenceCostForMakingPeace(Kingdom kingdom, bool forcePlayerCharacterCosts, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            var clanPayingInfluence = forcePlayerCharacterCosts ? Clan.PlayerClan : kingdom.Leader.Clan;
            InfluenceCost influenceCost;
            if (!Settings.Instance!.EnableInfluenceCostsForDiplomacyActions)
            {
                influenceCost = new InfluenceCost(clanPayingInfluence, 0f);
            }
            else if (!Settings.Instance!.ScalingInfluenceCosts)
            {
                influenceCost = new InfluenceCost(clanPayingInfluence, Settings.Instance!.MakePeaceInfluenceCost * GetPartyTypeFactor(kingdomPartyType));
            }
            else
            {
                influenceCost = new InfluenceCost(clanPayingInfluence, GetKingdomScalingFactor(kingdom) * PeaceFactor * GetPartyTypeFactor(kingdomPartyType));
            }

            return influenceCost;
        }

        private static float GetKingdomScalingFactor(Kingdom kingdom)
        {
            return (float)Math.Floor(GetKingdomTierCount(kingdom) * Settings.Instance!.ScalingInfluenceCostMultiplier);
        }

        public static HybridCost DetermineCostForFormingNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            return new(
                DetermineInfluenceCostForFormingNonAggressionPact(kingdom, otherKingdom, forcePlayerCharacterCosts, kingdomPartyType),
                DetermineGoldCostForFormingNonAggressionPact(kingdom, otherKingdom, forcePlayerCharacterCosts, kingdomPartyType));
        }

        public static InfluenceCost DetermineInfluenceCostForFormingNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            var clanPayingInfluence = forcePlayerCharacterCosts ? Clan.PlayerClan : kingdom.Leader.Clan;
            if (!Settings.Instance!.EnableInfluenceCostsForDiplomacyActions)
                return new InfluenceCost(clanPayingInfluence, 0f);
            if (!Settings.Instance!.ScalingInfluenceCosts)
                return new InfluenceCost(clanPayingInfluence, Settings.Instance!.MakePeaceInfluenceCost * GetPartyTypeFactor(kingdomPartyType));

            return new InfluenceCost(clanPayingInfluence, GetKingdomScalingFactor(kingdom) * GetPartyTypeFactor(kingdomPartyType));
        }

        public static GoldCost DetermineGoldCostForFormingNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            var giver = forcePlayerCharacterCosts ? Hero.MainHero : kingdom.Leader;

            var otherKingdomWarLoad = GetKingdomWarLoad(otherKingdom) + 1;

            var baseGoldCost = 500;
            var goldCostFactor = 100;
            var goldCost = (int)(((MBMath.ClampFloat((1 / otherKingdomWarLoad), 0f, 1f) * GetKingdomScalingFactor(kingdom) * goldCostFactor) + baseGoldCost) * GetPartyTypeFactor(kingdomPartyType));

            return new GoldCost(giver, null, goldCost); //This is not a bribing or payout, the money is spent by both parties to organize the process.
        }

        private static float GetKingdomWarLoad(Kingdom kingdom)
        {
            return FactionManager.GetEnemyFactions(kingdom)?.Select(x => x.TotalStrength).Aggregate(0f, (result, item) => result + item) / kingdom.TotalStrength ?? 0f;
        }

        public static DiplomacyCost DetermineCostForSendingMessenger()
        {
            return new GoldCost(Hero.MainHero, null, Settings.Instance!.SendMessengerGoldCost);
        }

        public static GoldCost DetermineGoldCostForMakingPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
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
                    var relativeWarExhaustion = (kingdomMakingPeaceWarExhaustion + 1f) / (otherKingdomWarExhaustion + 1f) - 1f;
                    warExhaustionMultiplier = MBMath.ClampFloat(relativeWarExhaustion, 0, ((1f / 20f) * kingdomMakingPeaceWarExhaustion) - 1f);
                }
                goldCost = Math.Min((int)(GetKingdomScalingFactor(kingdomMakingPeace) * warExhaustionMultiplier), kingdomMakingPeace.Leader.Gold / 2);

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

        public static HybridCost DetermineCostForFormingAlliance(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            return new(
                DetermineInfluenceCostForFormingAlliance(kingdom, forcePlayerCharacterCosts, kingdomPartyType),
                DetermineGoldCostForFormingAlliance(kingdom, otherKingdom, forcePlayerCharacterCosts, kingdomPartyType));
        }

        public static InfluenceCost DetermineInfluenceCostForFormingAlliance(Kingdom kingdom, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            var clanPayingInfluence = forcePlayerCharacterCosts ? Clan.PlayerClan : kingdom.Leader.Clan;
            if (!Settings.Instance!.EnableInfluenceCostsForDiplomacyActions)
                return new InfluenceCost(clanPayingInfluence, 0f);
            if (!Settings.Instance!.ScalingInfluenceCosts)
                return new InfluenceCost(clanPayingInfluence, Settings.Instance!.MakePeaceInfluenceCost * 2.0f * GetPartyTypeFactor(kingdomPartyType));

            return new InfluenceCost(clanPayingInfluence, GetKingdomScalingFactor(kingdom) * AllianceFactor * GetPartyTypeFactor(kingdomPartyType));
        }

        public static GoldCost DetermineGoldCostForFormingAlliance(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            var giver = forcePlayerCharacterCosts ? Hero.MainHero : kingdom.Leader;

            var otherKingdomWarLoad = GetKingdomWarLoad(otherKingdom) + 1;

            var baseGoldCost = 500;
            var goldCostFactor = 100;
            var goldCost = (int)(((MBMath.ClampFloat((1 / otherKingdomWarLoad), 0f, 1f) * GetKingdomScalingFactor(kingdom) * AllianceFactor * goldCostFactor) + baseGoldCost) * GetPartyTypeFactor(kingdomPartyType));

            return new GoldCost(giver, null, goldCost); //This is not a bribing or payout, the money is spent by both parties to organize the process.
        }

        private static float GetPartyTypeFactor(DiplomaticPartyType kingdomPartyType)
        {
            return kingdomPartyType switch
            {
                DiplomaticPartyType.Proposer => 1.0f,
                DiplomaticPartyType.Recipient => 0.5f,
                DiplomaticPartyType.ProposerInvolvee => 0.75f,
                DiplomaticPartyType.ReceiverInvolvee => 0.25f,
                _ => throw new ArgumentException(string.Format("{0} is not supported kingdom party type", kingdomPartyType.ToString()), nameof(kingdomPartyType)),
            };
        }
    }
}
