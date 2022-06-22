using Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace Diplomacy.Actions
{
    internal static class GiveGoldToClanAction
    {
        private static void ApplyInternal(Hero giverHero, Clan clan, int goldAmount)
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
            foreach (var valueTuple in MBMath.DistributeShares(gold, clan.Lords, new Func<Hero, int>(CalculateShare)))
            {
                GiveGoldAction.ApplyBetweenCharacters(null, valueTuple.Item1, valueTuple.Item2, false);
            }
        }

        private static int CalculateShare(Hero member)
        {
            return HeroHelper.CalculateTotalStrength(member) + 10;
        }

        public static void ApplyFromHeroToClan(Hero giverHero, Clan clan, int amount)
        {
            ApplyInternal(giverHero, clan, amount);
        }
    }
}
