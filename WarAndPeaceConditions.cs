using StoryMode.StoryModePhases;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace DiplomacyFixes
{
    class WarAndPeaceConditions
    {
        private const string NOT_ENOUGH_INFLUENCE = "{=TS1iV2pO}Not enough influence!";
        private const string NOT_ENOUGH_GOLD = "{=}Not enough gold!";
        private const string ACTIVE_QUEST = "{=XQFxDr11}There is an active quest preventing it!";
        private const string TOO_SOON = "{=ONNcmltF}This war hasn't gone on long enough to consider peace! It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.";
        private const string DECLARE_WAR_COOLDOWN = "{=jPHYDjXQ}Cannot declare war so soon after making peace! It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.";

        public static List<TextObject> CanMakePeaceExceptions(KingdomWarItemVM item)
        {
            return CanMakePeaceExceptions(item.Faction1 as Kingdom, item.Faction2 as Kingdom, true);
        }

        private static List<TextObject> CanMakePeaceExceptions(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            List<TextObject> exceptionList = new List<TextObject>();
            if (kingdomMakingPeace.Leader.IsHumanPlayerCharacter)
            {
                ThirdPhase thirdPhase = StoryMode.StoryMode.Current.MainStoryLine.ThirdPhase;
                bool isValidQuestState = thirdPhase == null || !thirdPhase.OppositionKingdoms.Contains(otherKingdom);
                if (!isValidQuestState)
                {
                    exceptionList.Add(new TextObject(ACTIVE_QUEST));
                }
            }
            Hero heroPayingCosts = forcePlayerCharacterCosts ? Hero.MainHero : kingdomMakingPeace.Leader;
            bool hasEnoughInfluence = heroPayingCosts.Clan.Influence >= DiplomacyCostCalculator.DetermineInfluenceCostForMakingPeace(kingdomMakingPeace);
            if (!hasEnoughInfluence)
            {
                exceptionList.Add(new TextObject(NOT_ENOUGH_INFLUENCE));
            }

            bool hasEnoughGold = heroPayingCosts.Gold >= DiplomacyCostCalculator.DetermineGoldCostForMakingPeace(kingdomMakingPeace, otherKingdom);
            if (!hasEnoughGold)
            {
                exceptionList.Add(new TextObject(NOT_ENOUGH_GOLD));
            }

            bool hasEnoughTimeElapsed = HasEnoughTimeElapsedBetweenWars(kingdomMakingPeace, otherKingdom, out float elapsedDaysUntilNow);
            if (!hasEnoughTimeElapsed)
            {
                TextObject textObject = new TextObject(TOO_SOON);
                textObject.SetTextVariable("ELAPSED_DAYS", (float)Math.Floor(elapsedDaysUntilNow));
                textObject.SetTextVariable("REQUIRED_DAYS", Settings.Instance.MinimumWarDurationInDays);
                exceptionList.Add(textObject);
            }

            return exceptionList;
        }

        public static List<TextObject> CanDeclareWarExceptions(KingdomTruceItemVM item)
        {
            return CanDeclareWarExceptions(item.Faction1 as Kingdom, item.Faction2 as Kingdom, true);
        }

        private static List<TextObject> CanDeclareWarExceptions(Kingdom kingdomDeclaringWar, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            List<TextObject> exceptionList = new List<TextObject>();
            Hero heroPayingCosts = forcePlayerCharacterCosts ? Hero.MainHero : kingdomDeclaringWar.Leader;
            bool hasEnoughInfluence = heroPayingCosts.Clan.Influence >= DiplomacyCostCalculator.DetermineInfluenceCostForDeclaringWar(kingdomDeclaringWar);
            if (!hasEnoughInfluence)
            {
                exceptionList.Add(new TextObject(NOT_ENOUGH_INFLUENCE));
            }

            CampaignTime? lastWarTimeWithPlayerFaction = CooldownManager.GetLastWarTimeWithPlayerFaction(otherKingdom);
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

        public static bool CanProposePeace(Kingdom kingdomProposingPeace, Kingdom otherKingdom)
        {
            return CanMakePeaceExceptions(kingdomProposingPeace, otherKingdom).IsEmpty();
        }

        public static bool CanDeclareWar(Kingdom kingdomDeclaringWar, Kingdom otherKingdom)
        {
            return CanDeclareWarExceptions(kingdomDeclaringWar, otherKingdom).IsEmpty();
        }

        private static bool HasEnoughTimeElapsedBetweenWars(IFaction faction1, IFaction faction2, out float elapsedTime)
        {
            elapsedTime = -1f;
            IEnumerable<CampaignWar> campaignWars = FactionManager.Instance.FindCampaignWarsBetweenFactions(faction1, faction2);
            if (campaignWars != null && campaignWars.Any())
            {
                foreach (CampaignWar war in campaignWars)
                {
                    elapsedTime = war.StartDate.ElapsedDaysUntilNow;
                    int minimumWarDurationInDays = Settings.Instance.MinimumWarDurationInDays;
                    return elapsedTime >= minimumWarDurationInDays;
                }
            }
            return true;
        }
    }
}
