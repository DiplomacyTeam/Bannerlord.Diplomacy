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
        private const string NOT_ENOUGH_GOLD = "{=IWZ91JVk}Not enough gold!";
        private const string ACTIVE_QUEST = "{=XQFxDr11}There is an active quest preventing it!";
        private const string TOO_SOON = "{=ONNcmltF}This war hasn't gone on long enough to consider peace! It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.";
        private const string DECLARE_WAR_COOLDOWN = "{=jPHYDjXQ}Cannot declare war so soon after making peace! It has only been {ELAPSED_DAYS} days out of a required {REQUIRED_DAYS} days.";
        private const string WAR_EXHAUSTION_TOO_HIGH = "{=QVp4v2MG}War exhaustion is too high to declare war. Current war exhaustion is {CURRENT_WAR_EXHAUSTION} and {LOW_WAR_EXHAUSTION_THRESHOLD} is the lowest allowed.";

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

            bool hasEnoughTimeElapsed = CooldownManager.HasExceededMinimumWarDuration(kingdomMakingPeace, otherKingdom, out float elapsedDaysUntilNow);
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

            bool hasDeclareWarCooldown = CooldownManager.HasDeclareWarCooldown(kingdomDeclaringWar, otherKingdom, out float elapsedTime);
            bool hasLowWarExhaustion = true;
            if (Settings.Instance.EnableWarExhaustion)
            {
                hasLowWarExhaustion = WarExhaustionManager.Instance.HasLowWarExhaustion(kingdomDeclaringWar, otherKingdom);
            }

            if (!hasLowWarExhaustion)
            {
                int lowWarExhaustionThreshold = (int) WarExhaustionManager.GetLowWarExhaustion();
                TextObject textObject = new TextObject(WAR_EXHAUSTION_TOO_HIGH);
                textObject.SetTextVariable("LOW_WAR_EXHAUSTION_THRESHOLD", lowWarExhaustionThreshold);
                textObject.SetTextVariable("CURRENT_WAR_EXHAUSTION", (int) Math.Ceiling(WarExhaustionManager.Instance.GetWarExhaustion(kingdomDeclaringWar, otherKingdom)));
                exceptionList.Add(textObject);
            }

            if (hasDeclareWarCooldown)
            {
                int declareWarCooldownDuration = Settings.Instance.DeclareWarCooldownInDays;

                TextObject textObject = new TextObject(DECLARE_WAR_COOLDOWN);
                textObject.SetTextVariable("ELAPSED_DAYS", (float)Math.Floor(elapsedTime));
                textObject.SetTextVariable("REQUIRED_DAYS", declareWarCooldownDuration);
                exceptionList.Add(textObject);
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
    }
}
