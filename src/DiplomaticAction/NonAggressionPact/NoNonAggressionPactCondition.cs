using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.NonAggressionPact
{
    internal class NoNonAggressionPactCondition : AbstractDiplomacyCondition
    {
        private static readonly TextObject _THasNonAggressionPact = new("{=fXcEtO29}Cannot have an active non-aggression pact.");

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            textObject = null;
            var hasNonAggressionPact = DiplomaticAgreementManager.HasNonAggressionPact(kingdom, otherKingdom, out _);
            if (hasNonAggressionPact)
            {
                textObject = _THasNonAggressionPact;
            }
            return !hasNonAggressionPact;
        }
    }
}
