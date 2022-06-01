using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.NonAggressionPact
{
    internal class NoNonAggressionPactCondition : IDiplomacyCondition
    {
        private static readonly TextObject _THasNonAggressionPact = new("{=fXcEtO29}Cannot have an active non-aggression pact.");
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
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