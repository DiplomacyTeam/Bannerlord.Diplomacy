using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.Alliance.Conditions
{
    class HasEnoughInfluenceCondition : IDiplomacyCondition
    {
        private const string NOT_ENOUGH_INFLUENCE = "{=TS1iV2pO}Not enough influence!";
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            float influenceCost = DiplomacyCostCalculator.DetermineInfluenceCostForFormingAlliance(kingdom, otherKingdom, true);
            bool hasEnoughInfluence;
            if (forcePlayerCharacterCosts)
            {
                hasEnoughInfluence = influenceCost <= Clan.PlayerClan.Influence;
            }
            else
            {
                hasEnoughInfluence = influenceCost <= kingdom.Leader.Clan.Influence;
            }

            if(!hasEnoughInfluence)
            {
                textObject = new TextObject(NOT_ENOUGH_INFLUENCE);
            }

            return hasEnoughInfluence;
        }
    }
}
