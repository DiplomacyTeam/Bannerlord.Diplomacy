using Diplomacy.Costs;
using Diplomacy.Event;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Alliance
{
    //TODO: We probably need to separate the act of breaking the alliance from its conformed dissolution (or the exclusion of a kingdom).
    //The first action should imply serious diplomatic and relation penalties to the kingdom that violates the agreement,
    //while the second should simulate an agreed termination of the treaty and should not impose any penalties,
    //but should be confirmed by all parties to the alliance and probably cost all parties some money and influence, similar to how forming the alliance does.
    internal sealed class BreakAllianceAction : AbstractDiplomaticAction<BreakAllianceAction>
    {
        public override bool PassesConditions(Kingdom kingdom, Kingdom otherKingdom, DiplomacyConditionType accountedConditions = DiplomacyConditionType.All, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer) =>
            BreakAllianceConditions.Instance.CanApply(kingdom, otherKingdom, accountedConditions, forcePlayerCharacterCosts, kingdomPartyType);

        protected override void ApplyInternal(Kingdom kingdom, Kingdom otherKingdom, float? customDurationInDays)
        {
            LogFactory.Get<DeclareAllianceAction>().LogTrace($"[{CampaignTime.Now}] {kingdom.Name} broke their alliance with {otherKingdom.Name}.");
            FactionManager.SetNeutral(kingdom, otherKingdom);
            Events.Instance.OnAllianceBroken(new AllianceEvent(kingdom, otherKingdom));
        }

        protected override void AssessCosts(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            if (kingdomPartyType == DiplomaticPartyType.Proposer)
            {
                DiplomacyCostCalculator.DetermineInfluenceCostForFormingAlliance(kingdom, forcePlayerCharacterCosts, kingdomPartyType).ApplyCost();
            }
        }

        protected override Dictionary<InvolvedParty, List<Kingdom>>? GetInvolvedKingdoms(Kingdom proposingKingdom, Kingdom otherKingdom)
        {
            if (TryGetIvolvees(proposingKingdom, otherKingdom, out var receivingPartyIvolvees))
            {
                Dictionary<InvolvedParty, List<Kingdom>> involvedKingdoms = new();
                if (receivingPartyIvolvees.Count > 0)
                {
                    involvedKingdoms.Add(InvolvedParty.ReceivingParty, receivingPartyIvolvees);
                }
                return involvedKingdoms;
            }
            else
            {
                return base.GetInvolvedKingdoms(proposingKingdom, otherKingdom); //null
            }

            bool TryGetIvolvees(Kingdom kingdom, Kingdom otherPartyKingdom, out List<Kingdom> involvedKingdoms)
            {
                involvedKingdoms = Kingdom.All.Where(k => k != kingdom && k != otherPartyKingdom
                                                          && FactionManager.IsAlliedWithFaction(k, kingdom)
                                                          && FactionManager.IsAlliedWithFaction(k, otherPartyKingdom)).ToList();
                return involvedKingdoms.Any();
            }
        }

        protected override void ShowPlayerInquiry(Kingdom proposingKingdom, Action acceptAction) => ShowPlayerInquiryInternal(proposingKingdom, acceptAction);

        protected override void ShowPlayerIvolvedInquiry(Kingdom proposingKingdom, Kingdom otherKingdom, InvolvedParty involvedParty, Action acceptAction)
        {
            if (involvedParty == InvolvedParty.ReceivingParty)
            {
                ShowPlayerInquiryInternal(proposingKingdom, acceptAction);
            }
        }

        private static void ShowPlayerInquiryInternal(Kingdom proposingKingdom, Action acceptAction)
        {
            var textObject = new TextObject("{=384jX28Q}{KINGDOM} is breaking the alliance with {PLAYER_KINGDOM}.");
            textObject.SetTextVariable("KINGDOM", proposingKingdom.Name);
            textObject.SetTextVariable("PLAYER_KINGDOM", Clan.PlayerClan.Kingdom.Name);

            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=D1ZQKZr1}Alliance Broken").ToString(),
                textObject.ToString(),
                true,
                false,
                GameTexts.FindText("str_ok").ToString(),
                null,
                acceptAction,
                null), true);
        }

        protected override float GetActionScoreInternal(Kingdom kingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType)
        {
            throw new NotImplementedException();
        }

        protected override float GetActionThreshold()
        {
            throw new NotImplementedException();
        }
    }
}
