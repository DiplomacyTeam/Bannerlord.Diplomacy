using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.NonAggressionPact
{
    class NoNonAggressionPactCondition : IDiplomacyCondition
    {
        private const string HAS_NON_AGGRESSION_PACT = "{=fXcEtO29}Cannot have an active non-aggression pact.";
        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            textObject = null;
            bool hasNonAggressionPact = DiplomaticAgreementManager.Instance.HasNonAggressionPact(kingdom, otherKingdom, out _);

            if (hasNonAggressionPact)
            {
                textObject = new TextObject(HAS_NON_AGGRESSION_PACT);
            }

            return !hasNonAggressionPact;
        }
    }
}
