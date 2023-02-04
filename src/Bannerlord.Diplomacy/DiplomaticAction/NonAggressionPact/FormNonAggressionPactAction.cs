using Diplomacy.Costs;

using Microsoft.Extensions.Logging;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.NonAggressionPact
{
    class FormNonAggressionPactAction : AbstractDiplomaticAction<FormNonAggressionPactAction>
    {
        public override bool PassesConditions(Kingdom proposingKingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            return NonAggressionPactConditions.Instance.CanApply(proposingKingdom, otherKingdom, forcePlayerCharacterCosts, bypassCosts);
        }

        protected override void ApplyInternal(Kingdom proposingKingdom, Kingdom otherKingdom, float? customDurationInDays)
        {
            LogFactory.Get<FormNonAggressionPactAction>()
                .LogTrace($"[{CampaignTime.Now}] {proposingKingdom.Name} secured a NAP with {otherKingdom.Name}.");
            DiplomaticAgreementManager.RegisterAgreement(proposingKingdom, otherKingdom,
                new NonAggressionPactAgreement(CampaignTime.Now,
                    CampaignTime.DaysFromNow(customDurationInDays ?? Settings.Instance!.NonAggressionPactDuration), proposingKingdom, otherKingdom));

            var textObject = new TextObject("{=vB3RrMNf}The {KINGDOM} has formed a non-aggression pact with the {OTHER_KINGDOM}.");
            textObject.SetTextVariable("KINGDOM", proposingKingdom.Name);
            textObject.SetTextVariable("OTHER_KINGDOM", otherKingdom.Name);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
        }

        protected override void AssessCosts(Kingdom proposingKingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts)
        {
            DiplomacyCostCalculator.DetermineCostForFormingNonAggressionPact(proposingKingdom, otherKingdom, forcePlayerCharacterCosts).ApplyCost();
        }

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
    }
}