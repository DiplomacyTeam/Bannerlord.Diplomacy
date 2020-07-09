using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.Alliance
{
    class DeclareAllianceAction : AbstractDiplomaticAction<DeclareAllianceAction>
    {
        public override bool PassesConditions(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            return FormAllianceConditions.Instance.CanApply(kingdom, kingdom, forcePlayerCharacterCosts, bypassCosts);
        }

        protected override void ApplyInternal(Kingdom kingdom, Kingdom otherKingdom)
        {
            FactionManager.DeclareAlliance(kingdom, otherKingdom);
            Events.Instance.OnAllianceFormed(new AllianceEvent(kingdom, otherKingdom));
        }

        protected override void AssessCosts(Kingdom proposingKingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts)
        {
            DiplomacyCostCalculator.DetermineCostForFormingAlliance(proposingKingdom, otherKingdom, forcePlayerCharacterCosts).ApplyCost();
        }

        protected override void ShowPlayerInquiry(Kingdom proposingKingdom, Action acceptAction)
        {
            TextObject textObject = new TextObject("{=QbOqatd7}{KINGDOM} is proposing an alliance with {PLAYER_KINGDOM}.");
            textObject.SetTextVariable("KINGDOM", proposingKingdom.Name);
            textObject.SetTextVariable("PLAYER_KINGDOM", Clan.PlayerClan.Kingdom.Name);

            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=3pbwc8sh}Alliance Proposal").ToString(),
                textObject.ToString(),
                true,
                true,
                new TextObject(StringConstants.Accept).ToString(),
                new TextObject(StringConstants.Decline).ToString(),
                acceptAction,
                null,
                ""), true);
        }
    }
}
