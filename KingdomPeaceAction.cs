using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace DiplomacyFixes
{
    class KingdomPeaceAction
    {

        private static void AcceptPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, int payment, float influenceCost)
        {
            DiplomacyCostManager.PayWarReparations(kingdomMakingPeace, otherKingdom, payment);
            DiplomacyCostManager.deductInfluenceFromKingdom(kingdomMakingPeace, influenceCost);
            MakePeaceAction.Apply(kingdomMakingPeace, otherKingdom);
        }

        private static void AcceptPeaceDueToWarExhaustion(Kingdom kingdomMakingPeace, Kingdom otherKingdom)
        {
            AcceptPeace(kingdomMakingPeace, otherKingdom, 0, 0f);
        }

        private static string CreateMakePeaceInquiryText(Kingdom kingdom, IFaction faction, int payment)
        {
            TextObject peaceText = new TextObject("{=t0ZS9maD}{KINGDOM_LEADER} of the {KINGDOM} is proposing peace with the {PLAYER_KINGDOM}.");
            peaceText.SetTextVariable("KINGDOM_LEADER", kingdom.Leader.Name.ToString());
            peaceText.SetTextVariable("KINGDOM", kingdom.Name.ToString());
            peaceText.SetTextVariable("PLAYER_KINGDOM", faction.Name.ToString());
            List<string> inquiryText = new List<string>();
            inquiryText.Add(peaceText.ToString());

            if (payment > 0)
            {
                TextObject warReparationText = new TextObject("{=ZrwszZww} They are willing to pay war reparations of {DENARS} denars.");
                warReparationText.SetTextVariable("DENARS", payment);
                inquiryText.Add(warReparationText.ToString());
            }
            return String.Concat(inquiryText);
        }

        private static string CreateMakePeaceDueToWarExhaustionInquiryText(Kingdom kingdom, IFaction faction)
        {
            TextObject peaceText = new TextObject("{=HWiDa4R1}Exhausted from the war, the {KINGDOM} is proposing peace with the {PLAYER_KINGDOM}.");
            peaceText.SetTextVariable("KINGDOM", kingdom.Name.ToString());
            peaceText.SetTextVariable("PLAYER_KINGDOM", faction.Name.ToString());

            return peaceText.ToString();
        }

        private static void CreatePeaceInquiry(Kingdom kingdom, Kingdom faction, int payment, float influenceCost)
        {
            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=BkGSVccZ}Peace Proposal").ToString(), CreateMakePeaceInquiryText(kingdom, faction, payment), true, true, new TextObject("{=3fTqLwkC}Accept").ToString(), new TextObject("{=dRoMejb0}Decline").ToString(), () =>
            {
                KingdomPeaceAction.AcceptPeace(kingdom, faction, payment, influenceCost);
            }, () =>
            {
                Events.Instance.OnPeaceProposalSent(kingdom);
            }, ""), true);
        }

        public static void ApplyPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool payCosts = true)
        {
            int payment = payCosts ? DiplomacyCostCalculator.DetermineGoldCostForMakingPeace(kingdomMakingPeace, otherKingdom) : 0;
            float influenceCost = payCosts ? DiplomacyCostCalculator.DetermineInfluenceCostForMakingPeace(kingdomMakingPeace) : 0f;
            ApplyPeace(kingdomMakingPeace, otherKingdom, payment, influenceCost);
        }

        public static void ApplyPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, int payment, float influenceCost)
        {
            if (!PlayerHelpers.IsPlayerLeaderOfFaction(otherKingdom))
            {
                KingdomPeaceAction.AcceptPeace(kingdomMakingPeace, otherKingdom, payment, influenceCost);
            }
            else
            {
                if (!CooldownManager.HasPeaceProposalCooldown(kingdomMakingPeace))
                {
                    KingdomPeaceAction.CreatePeaceInquiry(kingdomMakingPeace, otherKingdom, payment, influenceCost);
                }
            }
        }

        public static void ApplyPeaceDueToWarExhaustion(Kingdom kingdomMakingPeace, Kingdom otherKingdom)
        {
            if (!PlayerHelpers.IsPlayerLeaderOfFaction(otherKingdom))
            {
                KingdomPeaceAction.AcceptPeaceDueToWarExhaustion(kingdomMakingPeace, otherKingdom);
            }
            else
            {
                if (!CooldownManager.HasPeaceProposalCooldown(kingdomMakingPeace))
                {
                    KingdomPeaceAction.CreatePeaceInquiryDueToWarExhaustion(kingdomMakingPeace, otherKingdom);
                }
            }
        }

        private static void CreatePeaceInquiryDueToWarExhaustion(Kingdom kingdomMakingPeace, Kingdom otherKingdom)
        {
            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=BkGSVccZ}Peace Proposal").ToString(), CreateMakePeaceDueToWarExhaustionInquiryText(kingdomMakingPeace, otherKingdom), true, true, new TextObject("{=Y94H6XnK}Accept").ToString(), new TextObject("{=cOgmdp9e}Decline").ToString(), () =>
            {
                KingdomPeaceAction.AcceptPeaceDueToWarExhaustion(kingdomMakingPeace, otherKingdom);
            }, () =>
            {
                Events.Instance.OnPeaceProposalSent(kingdomMakingPeace);
            }, ""), true);
        }
    }
}
