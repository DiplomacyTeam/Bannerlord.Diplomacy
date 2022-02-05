using Diplomacy.Costs;
using Diplomacy.DiplomaticAction.Conditioning;

using Microsoft.Extensions.Logging;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.NonAggressionPact
{
    class FormNonAggressionPactAction : AbstractDiplomaticAction<FormNonAggressionPactAction>
    {
        public override bool PassesConditions(Kingdom kingdom, Kingdom otherKingdom, DiplomacyConditionType accountedConditions = DiplomacyConditionType.All, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer) =>
            NonAggressionPactConditions.Instance.CanApply(kingdom, otherKingdom, accountedConditions, forcePlayerCharacterCosts, kingdomPartyType);

        protected override void ApplyInternal(Kingdom kingdom, Kingdom otherKingdom, float? customDurationInDays)
        {
            LogFactory.Get<FormNonAggressionPactAction>().LogTrace($"[{CampaignTime.Now}] {kingdom.Name} secured a NAP with {otherKingdom.Name}.");
            DiplomaticAgreementManager.RegisterAgreement(new NonAggressionPactAgreement(CampaignTime.Now, CampaignTime.DaysFromNow(customDurationInDays ?? Settings.Instance!.NonAggressionPactDuration), kingdom, otherKingdom));

            var textObject = new TextObject("{=vB3RrMNf}The {KINGDOM} has formed a non-aggression pact with the {OTHER_KINGDOM}.");
            textObject.SetTextVariable("KINGDOM", kingdom.Name);
            textObject.SetTextVariable("OTHER_KINGDOM", otherKingdom.Name);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
        }

        protected override void AssessCosts(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer) =>
            DiplomacyCostCalculator.DetermineCostForFormingNonAggressionPact(kingdom, otherKingdom, forcePlayerCharacterCosts, kingdomPartyType).ApplyCost();

        protected override float GetActionScoreInternal(Kingdom kingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType) =>
            NonAggressionPactScoringModel.Instance.GetScore(kingdom, otherKingdom, kingdomPartyType).ResultNumber;

        protected override float GetActionThreshold() => NonAggressionPactScoringModel.AcceptOrProposeThreshold;

        protected override void ShowPlayerInquiry(Kingdom proposingKingdom, Action applyAction)
        {
            var textObject = new TextObject("{=gyLjlpJB}{KINGDOM} is proposing a non-aggression pact with {PLAYER_KINGDOM}.");
            textObject.SetTextVariable("KINGDOM", proposingKingdom.Name);
            textObject.SetTextVariable("PLAYER_KINGDOM", Clan.PlayerClan.Kingdom.Name);
            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=yj4XFa5T}Non-Aggression Pact Proposal").ToString(),
                textObject.ToString(),
                true,
                true,
                new TextObject(StringConstants.Accept).ToString(),
                new TextObject(StringConstants.Decline).ToString(),
                applyAction,
                null), true);
        }

        protected override void ShowPlayerIvolvedInquiry(Kingdom proposingKingdom, Kingdom otherKingdom, InvolvedParty involvedParty, Action acceptAction)
        {
            throw new NotImplementedException();
        }
    }
}