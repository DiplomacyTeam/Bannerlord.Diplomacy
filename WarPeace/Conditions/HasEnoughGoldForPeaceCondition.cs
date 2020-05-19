using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.WarPeace.Conditions
{
    class HasEnoughGoldForPeaceCondition : IDiplomacyCondition
    {
        private const string NOT_ENOUGH_GOLD = "{=IWZ91JVk}Not enough gold!";
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            textObject = null;
            Hero heroPayingCosts = forcePlayerCharacterCosts ? Hero.MainHero : kingdom.Leader;
            bool hasEnoughGold = heroPayingCosts.Gold >= DiplomacyCostCalculator.DetermineGoldCostForMakingPeace(kingdom, otherKingdom);
            if (!hasEnoughGold)
            {
                textObject = new TextObject(NOT_ENOUGH_GOLD);
            }
            return hasEnoughGold;
        }
    }
}
