using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.Alliance.Conditions
{
    class HasEnoughInfluenceCondition : IDiplomacyCondition
    {

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            bool hasEnoughInfluence = DiplomacyCostCalculator.DetermineCostForFormingAlliance(kingdom, otherKingdom, true).CanPayCost();
            if (!hasEnoughInfluence)
            {
                textObject = new TextObject(StringConstants.NOT_ENOUGH_INFLUENCE);
            }

            return hasEnoughInfluence;
        }
    }
}
