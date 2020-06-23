using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.WarPeace.Conditions
{
    class HasEnoughInfluenceForWarCondition : IDiplomacyCondition
    {
        private const string NOT_ENOUGH_INFLUENCE = "{=TS1iV2pO}Not enough influence!";

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;

            // Currently this cost is not assessed on AI leaders, but only through the Kingdom Diplomacy screen.
            if (!forcePlayerCharacterCosts)
            {
                return true;
            }

            Hero heroPayingCosts = forcePlayerCharacterCosts ? Hero.MainHero : kingdom.Leader;
            bool hasEnoughInfluence = heroPayingCosts.Clan.Influence >= DiplomacyCostCalculator.DetermineInfluenceCostForDeclaringWar(kingdom);
            if (!hasEnoughInfluence)
            {
                textObject = new TextObject(NOT_ENOUGH_INFLUENCE);
            }
            return hasEnoughInfluence;
        }
    }
}
