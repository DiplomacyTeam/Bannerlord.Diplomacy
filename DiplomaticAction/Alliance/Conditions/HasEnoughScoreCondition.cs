using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.Alliance.Conditions
{
    internal class HasEnoughScoreCondition : IDiplomacyCondition
    {
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            bool scoreTooLow = AllianceScoringModel.Instance.GetScore(otherKingdom, kingdom).ResultNumber < AllianceScoringModel.Instance.ScoreThreshold;
            if (scoreTooLow)
            {
                TextObject scoreTooLowText = new TextObject("{=VvTTrRpl}This faction is not interested in forming an alliance with you.");
                textObject = scoreTooLowText;
            }
            return !scoreTooLow;
        }
    }
}