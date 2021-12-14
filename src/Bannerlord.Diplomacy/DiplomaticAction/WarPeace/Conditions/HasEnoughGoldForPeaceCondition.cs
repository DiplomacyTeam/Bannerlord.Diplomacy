using Diplomacy.Costs;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace.Conditions
{
    internal sealed class HasEnoughGoldForPeaceCondition : AbstractCostCondition
    {
        private static readonly TextObject FailedConditionText = new(StringConstants.NotEnoughGold);

        protected override bool CanPayCost(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer) =>
            DiplomacyCostCalculator.DetermineGoldCostForMakingPeace(kingdom, otherKingdom, forcePlayerCharacterCosts, kingdomPartyType).CanPayCost();

        protected override TextObject GetFailedConditionText() => FailedConditionText;
    }
}