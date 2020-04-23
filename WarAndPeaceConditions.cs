using StoryMode.StoryModePhases;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;

namespace DiplomacyFixes
{
    class WarAndPeaceConditions
    {
        private const string NOT_ENOUGH_INFLUENCE = "Not enough influence!";
        private const string ACTIVE_QUEST = "There is an active quest preventing it!";
        private const string TOO_SOON = "This war hasn't gone on long enough to consider peace! It has only been {0} days out of a required {1} days.";
        private const string DECLARE_WAR_COOLDOWN = "Cannot declare war so soon after making peace! It has only been {0} days out of a required {1} days.";

        public static List<string> CanMakePeaceExceptions(KingdomWarItemVM item)
        {
            List<string> exceptionList = new List<string>();
            ThirdPhase thirdPhase = StoryMode.StoryMode.Current.MainStoryLine.ThirdPhase;
            bool isValidQuestState = thirdPhase == null || !thirdPhase.OppositionKingdoms.Contains(item.Faction2);
            if (!isValidQuestState)
            {
                exceptionList.Add(ACTIVE_QUEST);
            }
            bool hasEnoughInfluence = Hero.MainHero.Clan.Influence > CostCalculator.DetermineInfluenceCostForMakingPeace();
            if (!hasEnoughInfluence)
            {
                exceptionList.Add(NOT_ENOUGH_INFLUENCE);
            }

            IEnumerable<CampaignWar> campaignWars = FactionManager.Instance.FindCampaignWarsBetweenFactions(item.Faction1, item.Faction2);
            foreach (CampaignWar war in campaignWars)
            {
                if (war.AreFactionsAtWar(item.Faction1, item.Faction2))
                {
                    float elapsedDaysUntilNow = war.StartDate.ElapsedDaysUntilNow;
                    int minimumWarDurationInDays = Settings.Instance.MinimumWarDurationInDays;
                    bool hasEnoughTimeElapsed = elapsedDaysUntilNow >= minimumWarDurationInDays;
                    if (!hasEnoughTimeElapsed)
                    {
                        exceptionList.Add(string.Format(TOO_SOON, elapsedDaysUntilNow.ToString("0"), minimumWarDurationInDays));
                    }
                    break;
                }
            }
            return exceptionList;
        }

        public static List<string> CanDeclareWarExceptions(KingdomTruceItemVM item)
        {
            List<string> exceptionList = new List<string>();
            bool hasEnoughInfluence = Hero.MainHero.Clan.Influence > CostCalculator.DetermineInfluenceCostForDeclaringWar();
            if (!hasEnoughInfluence)
            {
                exceptionList.Add(NOT_ENOUGH_INFLUENCE);
            }

            if (CooldownManager.GetLastWarTimeWithFaction(item.Faction2).HasValue)
            {
                float elapsedDaysUntilNow = CooldownManager.GetLastWarTimeWithFaction(item.Faction2).Value.ElapsedDaysUntilNow;
                int declareWarCooldownInDays = Settings.Instance.DeclareWarCooldownInDays;
                bool hasEnoughTimeElapsed = elapsedDaysUntilNow >= declareWarCooldownInDays;
                if (!hasEnoughTimeElapsed)
                {
                    exceptionList.Add(string.Format(DECLARE_WAR_COOLDOWN, elapsedDaysUntilNow.ToString("0"), declareWarCooldownInDays));
                }
            }
            return exceptionList;
        }
    }
}
