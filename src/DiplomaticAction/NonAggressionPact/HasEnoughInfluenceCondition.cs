using Diplomacy.Costs;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.NonAggressionPact
{
    internal sealed class HasEnoughInfluenceCondition : AbstractCostCondition
    {
        private static readonly TextObject FailedConditionText = new(StringConstants.NotEnoughInfluence);

        protected override bool CanPayCost(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer) =>
            DiplomacyCostCalculator.DetermineInfluenceCostForFormingNonAggressionPact(kingdom, otherKingdom, forcePlayerCharacterCosts, kingdomPartyType).CanPayCost();

        protected override TextObject GetFailedConditionText() => FailedConditionText;
    }
}
