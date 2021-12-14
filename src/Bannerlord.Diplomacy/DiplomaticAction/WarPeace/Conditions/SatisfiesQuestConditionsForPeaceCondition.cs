using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace.Conditions
{
    internal class SatisfiesQuestConditionsForPeaceCondition : AbstractDiplomacyCondition
    {
        private static readonly TextObject _TActiveQuest = new("{=XQFxDr11}There is an active quest preventing it!");
        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            textObject = null;
#if e170
            var currentStoryMode = StoryMode.StoryModeManager.Current;
#else
            var currentStoryMode = StoryMode.StoryMode.Current;
#endif
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