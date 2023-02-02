using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Diplomacy.Actions
{
    public static class GiveGoldToKingdomAction
    {
        private static void ApplyInternal(Kingdom? giverKingdom, Kingdom? receiverKingdom, int amount, WalletType giverWallet, WalletType receiverWallet)
        {
            if (amount == 0)
                return;

            if (giverKingdom != null)
            {
                switch (giverWallet)
                {
                    case WalletType.TributeWallet:
                        giverKingdom.TributeWallet -= amount;
                        break;
                    case WalletType.BudgetWallet:
                        //Leaders should benefit from those payments too
                        var leaderAmount = amount < 0 ? amount / 3 : 0;
                        if (leaderAmount > 0) GiveGoldAction.ApplyBetweenCharacters(null, giverKingdom.Leader, leaderAmount);
                        giverKingdom.KingdomBudgetWallet -= (amount - leaderAmount);
                        break;
                    default:
                        break;
                }
            }
            if (receiverKingdom != null)
            {
                switch (receiverWallet)
                {
                    case WalletType.TributeWallet:
                        receiverKingdom.TributeWallet += amount;
                        break;
                    case WalletType.BudgetWallet:
                        //Leaders should benefit from those payments too
                        var leaderAmount = amount > 0 ? amount / 3 : 0;
                        if (leaderAmount > 0) GiveGoldAction.ApplyBetweenCharacters(null, receiverKingdom.Leader, leaderAmount);
                        receiverKingdom.KingdomBudgetWallet += (amount - leaderAmount);
                        break;
                    default:
                        break;
                }
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
            foreach ((var recipientClan, var amount) in MBMath.DistributeShares(gold, kingdom.Clans, CalculateShare))
            {
                GiveGoldToClanAction.ApplyToClan(recipientClan, amount);
            }
        }

        private static int CalculateShare(Clan clan)
        {
            return (int) clan.TotalStrength + (clan == clan.Kingdom?.Leader?.Clan ? 1000 : 10);
        }

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
        }
    }
}
