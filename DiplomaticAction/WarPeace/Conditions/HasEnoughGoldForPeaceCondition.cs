using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.WarPeace.Conditions
{
    class HasEnoughGoldForPeaceCondition : AbstractCostCondition
    {
        protected override TextObject FailedConditionText => new TextObject(StringConstants.NOT_ENOUGH_GOLD);

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, ref TextObject textObject, bool forcePlayerCharacterCosts = false)
        {
            Hero heroPayingCosts = forcePlayerCharacterCosts ? Hero.MainHero : kingdom.Leader;
            bool hasEnoughGold = heroPayingCosts.Gold >= DiplomacyCostCalculator.DetermineGoldCostForMakingPeace(kingdom, otherKingdom);
            if (!hasEnoughGold)
            {
                textObject = FailedConditionText;
            }
            return hasEnoughGold;
        }
    }
}
