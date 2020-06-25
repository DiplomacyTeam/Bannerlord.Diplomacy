using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.NonAggressionPact
{
    class HasEnoughGoldCondition : IDiplomacyCondition
    {
        private const string NOT_ENOUGH_GOLD = "{=IWZ91JVk}Not enough gold!";
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            bool hasEnoughGold = DiplomacyCostCalculator.DetermineCostForFormingNonAggressionPact(kingdom, otherKingdom, forcePlayerCharacterCosts).GoldCost.CanPayCost();
            if (!hasEnoughGold)
            {
                textObject = new TextObject(NOT_ENOUGH_GOLD);
            }
            return hasEnoughGold;
        }
    }
}
