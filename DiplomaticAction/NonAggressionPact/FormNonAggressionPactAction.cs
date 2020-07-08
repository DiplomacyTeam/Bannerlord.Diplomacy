using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.NonAggressionPact
{
    public class FormNonAggressionPactAction
    {
        public static void Apply(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            Kingdom playerKingdom = Clan.PlayerClan?.Kingdom;
            if (otherKingdom == playerKingdom && playerKingdom.Leader == Hero.MainHero)
            {
                TextObject textObject = new TextObject("{=gyLjlpJB}{KINGDOM} is proposing a non-aggression pact with {PLAYER_KINGDOM}.");
                textObject.SetTextVariable("KINGDOM", kingdom.Name);
                textObject.SetTextVariable("PLAYER_KINGDOM", otherKingdom.Name);
                InformationManager.ShowInquiry(new InquiryData(
                    new TextObject("{=yj4XFa5T}Non-Aggression Pact Proposal").ToString(),
                    textObject.ToString(),
                    true,
                    true,
                    new TextObject("{=3fTqLwkC}Accept").ToString(),
                    new TextObject("{=dRoMejb0}Decline").ToString(),
                    () => ApplyInternal(kingdom, otherKingdom, forcePlayerCharacterCosts, bypassCosts),
                    null,
                    ""), true);
            }
            else
            {
                ApplyInternal(kingdom, otherKingdom, forcePlayerCharacterCosts, bypassCosts);
            }

        }

        private static void ApplyInternal(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts, bool bypassCosts)
        {
            if (!bypassCosts)
            {
                DiplomacyCostCalculator.DetermineCostForFormingNonAggressionPact(kingdom, otherKingdom, forcePlayerCharacterCosts).ApplyCost();
            }

            DiplomaticAgreementManager.Instance.RegisterAgreement(kingdom, otherKingdom, new NonAggressionPactAgreement(CampaignTime.Now, CampaignTime.DaysFromNow(Settings.Instance.NonAggressionPactDuration), kingdom, otherKingdom));

            TextObject textObject = new TextObject("{=vB3RrMNf}The {KINGDOM} has formed a non-aggression pact with the {OTHER_KINGDOM}.");
            textObject.SetTextVariable("KINGDOM", kingdom.Name);
            textObject.SetTextVariable("OTHER_KINGDOM", otherKingdom.Name);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
        }
    }
}
