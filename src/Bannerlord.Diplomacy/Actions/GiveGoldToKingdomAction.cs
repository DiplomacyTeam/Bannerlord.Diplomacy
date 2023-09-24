using System;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Diplomacy.Actions
{
    public static class GiveGoldToKingdomAction
    {
        private const int MinRequiredBudgetWalletSize = 2000000;
        private const int MaxRequiredClanGold = 50000;
        private const int MinRequiredShortfall = 100000;
        private const int GoldPerProsperity = 1000;
        private const int MaxRevenuePerMercenaryTier = 10000;

        private static void ApplyInternal(Kingdom? giverKingdom, Kingdom? receiverKingdom, int amount, WalletType giverWallet, WalletType receiverWallet)
        {
            if (amount == 0)
                return;

            if (giverKingdom != null)
            {
                GetMoneyFromGiver(giverKingdom, amount, giverWallet);
            }
            if (receiverKingdom != null)
            {
                GiveMoneyToReceiver(receiverKingdom, amount, receiverWallet);
            }
        }

        private static void GetMoneyFromGiver(Kingdom giverKingdom, int amount, WalletType giverWallet)
        {
            switch (giverWallet)
            {
                case WalletType.TributeWallet:
                    giverKingdom.TributeWallet -= amount;
                    return;
                case WalletType.BudgetWallet:
                    giverKingdom.KingdomBudgetWallet -= amount;
                    return;
                case WalletType.ReparationsWallet:
                    //Cover from the kingdom budget
                    var availableBudgetAmount = Math.Max(giverKingdom.KingdomBudgetWallet - MinRequiredBudgetWalletSize, 0);
                    if (availableBudgetAmount > 0)
                    {
                        var budgetCoveredAmount = Math.Min(availableBudgetAmount, amount);
                        giverKingdom.KingdomBudgetWallet -= budgetCoveredAmount;
                        amount -= budgetCoveredAmount;
                    }
                    if (amount <= 0)
                        return;

                    //Cover from the kingdom prosperity
                    var tolerableAmount = giverKingdom.Clans.Where(c => !c.IsUnderMercenaryService && !c.IsEliminated).Sum(c => c.Gold - Math.Min(c.Gold / 4, MaxRequiredClanGold));
                    if (tolerableAmount < amount)
                    {
                        var amountToCover = amount - tolerableAmount;
                        if (amountToCover >= MinRequiredShortfall)
                        {
                            foreach ((var settlement, var amountToCoverBySettlement) in MBMath.DistributeShares(amountToCover, giverKingdom.Settlements.Where(s => s.IsCastle || s.IsTown), CalculateSettlementShare))
                            {
#if v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115 || v116
                                settlement.Prosperity -= amountToCoverBySettlement / GoldPerProsperity;
#else
                                settlement.Town.Prosperity -= amountToCoverBySettlement / GoldPerProsperity;
#endif
                            }
                            amount = tolerableAmount;
                        }
                    }

                    giverKingdom.TributeWallet -= amount;
                    return;
                default:
                    return;
            }
        }

        private static void GiveMoneyToReceiver(Kingdom receiverKingdom, int amount, WalletType receiverWallet)
        {
            switch (receiverWallet)
            {
                case WalletType.TributeWallet:
                    receiverKingdom.TributeWallet += amount;
                    return;
                case WalletType.BudgetWallet:
                    receiverKingdom.KingdomBudgetWallet += amount;
                    return;
                case WalletType.ReparationsWallet:
                    //Leaders should benefit from those payments too
                    var leaderAmount = amount / 3;
                    if (leaderAmount > 0) GiveGoldAction.ApplyBetweenCharacters(null, receiverKingdom.Leader, leaderAmount);
                    //Mercenaries
                    var mercenaryAmount = amount / 6;
                    int mercenaryAmountFact = 0;
                    foreach ((var recipientMercClan, var amountForMercClan) in MBMath.DistributeShares(mercenaryAmount, receiverKingdom.Clans.Where(c => c.IsUnderMercenaryService && !c.IsEliminated), CalculateMercenaryShare))
                    {
                        var amountForMercClanFact = Math.Min(amountForMercClan, recipientMercClan.Tier * MaxRevenuePerMercenaryTier);
                        GiveGoldAction.ApplyBetweenCharacters(null, recipientMercClan.Leader, amountForMercClanFact);
                        mercenaryAmountFact += amountForMercClanFact;
                    }
                    //Reassess
                    amount = amount - leaderAmount - mercenaryAmountFact;
                    //Player
                    var playerClan = Clan.PlayerClan;
                    if (playerClan.Kingdom == receiverKingdom && !playerClan.IsUnderMercenaryService && playerClan.Leader != receiverKingdom.Leader)
                    {
                        var playerAmount = MBMath.DistributeShares(amount, receiverKingdom.Clans.Where(c => !c.IsUnderMercenaryService && !c.IsEliminated), CalculateShare).FirstOrDefault(x => x.Item1 == playerClan);
                        if (playerAmount != default && playerAmount.Item2 > 0)
                        {
                            GiveGoldAction.ApplyBetweenCharacters(null, playerClan.Leader, playerAmount.Item2);
                            amount -= playerAmount.Item2;
                        }
                    }

                    receiverKingdom.KingdomBudgetWallet += amount;
                    return;
                default:
                    return;
            }
        }

        private static void ApplyInternal(Hero? giverHero, Kingdom? kingdom, int goldAmount)
        {
            if (giverHero != null)
            {
                GiveGoldAction.ApplyBetweenCharacters(giverHero, null, goldAmount, true);
            }
            if (kingdom != null)
            {
                GiveGoldToKingdom(goldAmount, kingdom);
            }
        }

        private static void GiveGoldToKingdom(int gold, Kingdom kingdom)
        {
            foreach ((var recipientClan, var amount) in MBMath.DistributeShares(gold, kingdom.Clans.Where(c => !c.IsUnderMercenaryService && !c.IsEliminated), CalculateShare))
            {
                GiveGoldToClanAction.ApplyToClan(recipientClan, amount);
            }
        }

        private static int CalculateShare(Clan clan) => Math.Max(clan.Tier / 2, 1) + (clan == clan.Kingdom?.Leader?.Clan ? 1 : 0);
        private static int CalculateMercenaryShare(Clan clan) => Math.Max((int) clan.Influence, 1);
#if v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115 || v116
        private static int CalculateSettlementShare(Settlement settlement) => Math.Max((int) settlement.Prosperity, 1);
#else
        private static int CalculateSettlementShare(Settlement settlement) => Math.Max((int) settlement.Town.Prosperity, 1);
#endif

        public static void ApplyFromHeroToKingdom(Hero giverHero, Kingdom kingdom, int amount)
        {
            ApplyInternal(giverHero, kingdom, amount);
        }

        public static void ApplyFromWalletToWallet(Kingdom? giverKingdom, Kingdom? receiverKingdom, int amount, WalletType giverWallet = WalletType.TributeWallet, WalletType receiverWallet = WalletType.BudgetWallet)
        {
            ApplyInternal(giverKingdom, receiverKingdom, amount, giverWallet, receiverWallet);
        }

        public enum WalletType : byte
        {
            None = 0,
            MercenaryWallet = 1,
            TributeWallet = 2,
            BudgetWallet = 3,
            ReparationsWallet = 4
        }
    }
}