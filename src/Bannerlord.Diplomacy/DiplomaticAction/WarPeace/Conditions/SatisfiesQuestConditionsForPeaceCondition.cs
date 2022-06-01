using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace.Conditions
{
    internal class SatisfiesQuestConditionsForPeaceCondition : IDiplomacyCondition
    {
        private static readonly TextObject _TActiveQuest = new("{=XQFxDr11}There is an active quest preventing it!");
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            textObject = null;
            var currentStoryMode = StoryMode.StoryModeManager.Current;
            // not in story mode
            if (currentStoryMode == null)
            {
                return true;
            }

            var thirdPhase = currentStoryMode?.MainStoryLine?.ThirdPhase;
            var isValidQuestState = thirdPhase is null || !thirdPhase.OppositionKingdoms.Contains(otherKingdom);
            if (!isValidQuestState)
            {
                textObject = _TActiveQuest;
            }
            return isValidQuestState;
        }
    }
}