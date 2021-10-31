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

            // not in story mode
            if (StoryMode.StoryMode.Current == null)
            {
                return true;
            }

            var thirdPhase = StoryMode.StoryMode.Current?.MainStoryLine?.ThirdPhase;
            var isValidQuestState = thirdPhase is null || !thirdPhase.OppositionKingdoms.Contains(otherKingdom);
            if (!isValidQuestState)
            {
                textObject = _TActiveQuest;
            }
            return isValidQuestState;
        }
    }
}
