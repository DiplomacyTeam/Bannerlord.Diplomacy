using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace DiplomacyFixes
{
    class KingdomPeaceAction
    {

        private static void AcceptPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, int payment, float influenceCost, bool forcePlayerCharacterCosts)
        {
            DiplomacyCostManager.PayWarReparations(kingdomMakingPeace, otherKingdom, payment);
            if (forcePlayerCharacterCosts)
            {
                DiplomacyCostManager.deductInfluenceFromPlayerClan(influenceCost);
            }
            else
            {
                DiplomacyCostManager.DeductInfluenceFromKingdom(kingdomMakingPeace, influenceCost);
            }
            MakePeaceAction.Apply(kingdomMakingPeace, otherKingdom);
        }

        private static string CreateMakePeaceInquiryText(Kingdom kingdomMakingPeace, Kingdom otherKingdom, int payment)
        {
            TextObject peaceText;
            if (Settings.Instance.EnableWarExhaustion && WarExhaustionManager.Instance.HasMaxWarExhaustion(kingdomMakingPeace, otherKingdom))
            {
                peaceText = new TextObject("{=HWiDa4R1}Exhausted from the war, the {KINGDOM} is proposing peace with the {PLAYER_KINGDOM}.");
                peaceText.SetTextVariable("KINGDOM", kingdomMakingPeace.Name.ToString());
                peaceText.SetTextVariable("PLAYER_KINGDOM", otherKingdom.Name.ToString());
            }
            else
            {
                peaceText = new TextObject("{=t0ZS9maD}{KINGDOM_LEADER} of the {KINGDOM} is proposing peace with the {PLAYER_KINGDOM}.");
                peaceText.SetTextVariable("KINGDOM_LEADER", kingdomMakingPeace.Leader.Name.ToString());
                peaceText.SetTextVariable("KINGDOM", kingdomMakingPeace.Name.ToString());
                peaceText.SetTextVariable("PLAYER_KINGDOM", otherKingdom.Name.ToString());
            }
            List<string> inquiryText = new List<string>();
            inquiryText.Add(peaceText.ToString());

            if (payment > 0)
            {
                TextObject warReparationText = new TextObject("{=ZrwszZww} They are willing to pay war reparations of {DENARS} denars.");
                warReparationText.SetTextVariable("DENARS", payment);
                inquiryText.Add(warReparationText.ToString());
            }
            return string.Concat(inquiryText);
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
                KingdomPeaceAction.AcceptPeace(kingdom, faction, payment, influenceCost, false);
            }, () =>
            {
                Events.Instance.OnPeaceProposalSent(kingdom);
            }, ""), true);
        }

        public static void ApplyPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool payCosts = true, bool forcePlayerCharacterCosts = false)
        {
            int payment = payCosts ? DiplomacyCostCalculator.DetermineGoldCostForMakingPeace(kingdomMakingPeace, otherKingdom) : 0;
            float influenceCost = payCosts ? DiplomacyCostCalculator.DetermineInfluenceCostForMakingPeace(kingdomMakingPeace) : 0f;
            ApplyPeace(kingdomMakingPeace, otherKingdom, payment, influenceCost, forcePlayerCharacterCosts);
        }

        public static void ApplyPeace(Kingdom kingdomMakingPeace, Kingdom otherKingdom, int payment, float influenceCost, bool forcePlayerCharacterCosts = false)
        {
            if (!otherKingdom.Leader.IsHumanPlayerCharacter)
            {
                KingdomPeaceAction.AcceptPeace(kingdomMakingPeace, otherKingdom, payment, influenceCost, forcePlayerCharacterCosts);
            }
            else if (!CooldownManager.HasPeaceProposalCooldownWithPlayerKingdom(kingdomMakingPeace))
            {
                KingdomPeaceAction.CreatePeaceInquiry(kingdomMakingPeace, otherKingdom, payment, influenceCost);
            }
        }
    }
}
