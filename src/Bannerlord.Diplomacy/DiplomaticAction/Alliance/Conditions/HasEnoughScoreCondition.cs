using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Alliance.Conditions
{
    internal class HasEnoughScoreCondition : IDiplomacyCondition
    {
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            textObject = null;
            var scoreTooLow = AllianceScoringModel.Instance.GetScore(otherKingdom, kingdom).ResultNumber < AllianceScoringModel.Instance.ScoreThreshold;
            if (scoreTooLow)
            {
                var scoreTooLowText = new TextObject("{=VvTTrRpl}This faction is not interested in forming an alliance with you.");
                textObject = scoreTooLowText;
            }
            return !scoreTooLow;
        }
    }
}