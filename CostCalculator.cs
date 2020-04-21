using MBOptionScreen.Settings;
using StoryMode.CharacterCreationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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

            return getKingdomTierCount() * 5;
        }

        public static float determineInfluenceCostForMakingPeace()
        {
            if (!Settings.Instance.EnableInfluenceCostsForDiplomacyActions)
                return 0f;
            if (!Settings.Instance.ScalingInfluenceCosts)
                return Settings.Instance.MakePeaceInfluenceCost;

            return getKingdomTierCount() * 5;
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
