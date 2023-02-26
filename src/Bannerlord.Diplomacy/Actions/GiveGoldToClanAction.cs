using Helpers;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace Diplomacy.Actions
{
    internal static class GiveGoldToClanAction
    {
        private static void ApplyInternal(Hero? giverHero, Clan? clan, int goldAmount)
        {
            if (giverHero != null)
            {
                GiveGoldAction.ApplyBetweenCharacters(giverHero, null, goldAmount, true);
            }
            if (clan != null)
            {
                GiveGoldToClan(goldAmount, clan);
            }
        }

        private static void GiveGoldToClan(int gold, Clan clan)
        {
            foreach ((var recipientHero, var amount) in MBMath.DistributeShares(gold, clan.Lords.Where(l => l.IsAlive && l.IsCommander), CalculateShare))
            {
                GiveGoldAction.ApplyBetweenCharacters(null, recipientHero, amount);
            }
        }

        private static int CalculateShare(Hero member)
        {
            return HeroHelper.CalculateTotalStrength(member) + (member == member.Clan?.Leader ? 500 : 10);
        }

        public static void ApplyFromHeroToClan(Hero giverHero, Clan clan, int amount)
        {
            ApplyInternal(giverHero, clan, amount);
        }

        public static void ApplyToClan(Clan clan, int amount)
        {
            ApplyInternal(null, clan, amount);
        }
    }
}