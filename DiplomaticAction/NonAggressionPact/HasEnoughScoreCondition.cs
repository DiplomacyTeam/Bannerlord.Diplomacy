
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.NonAggressionPact
{
    class HasEnoughScoreCondition : IDiplomacyCondition
    {
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            bool scoreTooLow = NonAggressionPactScoringModel.GetFormNonAggressionPactScore(otherKingdom, kingdom).ResultNumber < NonAggressionPactScoringModel.FormNonAggressionPactThreshold;
            if (scoreTooLow)
            {
                TextObject scoreTooLowText = new TextObject("{=M4SGjzQP}This faction is not interested in forming a non-aggression pact with you.");
                textObject = scoreTooLowText;
            }
            return !scoreTooLow;
        }
    }
}
