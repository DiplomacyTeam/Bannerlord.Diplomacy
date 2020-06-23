using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.NonAggressionPact.Conditions
{
    class HasEnoughInfluenceCondition : IDiplomacyCondition
    {
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            float influenceCost = DiplomacyCostCalculator.DetermineInfluenceCostForFormingNonAggressionPact(kingdom);
            bool hasEnoughInfluence;
            if (forcePlayerCharacterCosts)
            {
                hasEnoughInfluence = influenceCost <= Clan.PlayerClan.Influence;
            }
            else
            {
                hasEnoughInfluence = influenceCost <= kingdom.Leader.Clan.Influence;
            }

            if (!hasEnoughInfluence)
            {
                textObject = new TextObject(StringConstants.NOT_ENOUGH_INFLUENCE);
            }

            return hasEnoughInfluence;
        }
    }
}
