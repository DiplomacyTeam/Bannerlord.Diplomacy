using StoryMode.StoryModePhases;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;

namespace DiplomacyFixes
{
    class WarAndPeaceConditions
    {
        private const string NOT_ENOUGH_INFLUENCE = "Not enough influence!";
        private const string ACTIVE_QUEST = "There is an active quest preventing it!";

        public static List<string> canMakePeaceExceptions(KingdomWarItemVM item)
        {
            List<string> exceptionList = new List<string>();
            ThirdPhase thirdPhase = StoryMode.StoryMode.Current.MainStoryLine.ThirdPhase;
            bool isValidQuestState = thirdPhase == null || !thirdPhase.OppositionKingdoms.Contains(item.Faction2);
            if(!isValidQuestState)
            {
                exceptionList.Add(ACTIVE_QUEST);
            }
            bool hasEnoughInfluence = Hero.MainHero.Clan.Influence > CostCalculator.determineInfluenceCostForMakingPeace();
            if (!hasEnoughInfluence)
            {
                exceptionList.Add(NOT_ENOUGH_INFLUENCE);
            }

            return exceptionList;
        }

        public static List<string> canDeclareWarExceptions(KingdomTruceItemVM item)
        {
            List<string> exceptionList = new List<string>();
            bool hasEnoughInfluence = Hero.MainHero.Clan.Influence > CostCalculator.determineInfluenceCostForDeclaringWar();
            if (!hasEnoughInfluence)
            {
                exceptionList.Add(NOT_ENOUGH_INFLUENCE);
            }

            return exceptionList;
        }
    }
}
