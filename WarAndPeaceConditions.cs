using StoryMode.StoryModePhases;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Localization;

namespace DiplomacyFixes
{
    class WarAndPeaceConditions
    {
        private const string NOT_ENOUGH_INFLUENCE = "{=TS1iV2pO}Not enough influence!";
        private const string ACTIVE_QUEST = "{=XQFxDr11}There is an active quest preventing it!";
        private const string TOO_SOON = "{=ONNcmltF}This war hasn't gone on long enough to consider peace! It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.";
        private const string DECLARE_WAR_COOLDOWN = "{=jPHYDjXQ}Cannot declare war so soon after making peace! It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.";

        public static List<TextObject> CanMakePeaceExceptions(KingdomWarItemVM item)
        {
            List<TextObject> exceptionList = new List<TextObject>();
            ThirdPhase thirdPhase = StoryMode.StoryMode.Current.MainStoryLine.ThirdPhase;
            bool isValidQuestState = thirdPhase == null || !thirdPhase.OppositionKingdoms.Contains(item.Faction2);
            if (!isValidQuestState)
            {
                exceptionList.Add(new TextObject(ACTIVE_QUEST));
            }
            bool hasEnoughInfluence = Hero.MainHero.Clan.Influence > DiplomacyCostCalculator.DetermineInfluenceCostForMakingPeace();
            if (!hasEnoughInfluence)
            {
                exceptionList.Add(new TextObject(NOT_ENOUGH_INFLUENCE));
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
                        TextObject textObject = new TextObject(TOO_SOON);
                        textObject.SetTextVariable("ELAPSED_DAYS", (float)Math.Floor(elapsedDaysUntilNow));
                        textObject.SetTextVariable("REQUIRED_DAYS", minimumWarDurationInDays);
                        exceptionList.Add(textObject);
                    }
                    break;
                }
            }
            return exceptionList;
        }

        public static List<TextObject> CanDeclareWarExceptions(KingdomTruceItemVM item)
        {
            List<TextObject> exceptionList = new List<TextObject>();
            bool hasEnoughInfluence = Hero.MainHero.Clan.Influence > DiplomacyCostCalculator.DetermineInfluenceCostForDeclaringWar();
            if (!hasEnoughInfluence)
            {
                exceptionList.Add(new TextObject(NOT_ENOUGH_INFLUENCE));
            }

            CampaignTime? lastWarTimeWithPlayerFaction = CooldownManager.GetLastWarTimeWithPlayerFaction(item.Faction2);
            if (lastWarTimeWithPlayerFaction.HasValue)
            {
                float elapsedDaysUntilNow = lastWarTimeWithPlayerFaction.Value.ElapsedDaysUntilNow;
                int declareWarCooldownInDays = Settings.Instance.DeclareWarCooldownInDays;
                bool hasEnoughTimeElapsed = elapsedDaysUntilNow >= declareWarCooldownInDays;
                if (!hasEnoughTimeElapsed)
                {
                    TextObject textObject = new TextObject(DECLARE_WAR_COOLDOWN);
                    textObject.SetTextVariable("ELAPSED_DAYS", (float)Math.Floor(elapsedDaysUntilNow));
                    textObject.SetTextVariable("REQUIRED_DAYS", declareWarCooldownInDays);
                    exceptionList.Add(textObject);
                }
            }
            return exceptionList;
        }
    }
}
