using Diplomacy.Costs;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Alliance.Conditions
{
    internal sealed class HasEnoughGoldCondition : AbstractCostCondition
    {
        private static readonly TextObject FailedConditionText = new(StringConstants.NotEnoughGold);

        protected override bool CanPayCost(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer) =>
            DiplomacyCostCalculator.DetermineGoldCostForFormingAlliance(kingdom, otherKingdom, forcePlayerCharacterCosts, kingdomPartyType).CanPayCost();

        protected override TextObject GetFailedConditionText() => FailedConditionText;
    }
}