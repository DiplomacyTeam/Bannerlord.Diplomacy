using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.TwoDimension;

namespace DiplomacyFixes
{
    class KingdomPeaceAction
    {
        private static void AcceptPeace(Kingdom kingdom, IFaction faction, int payment)
        {
            GiveGoldAction.ApplyBetweenCharacters(kingdom.Leader, faction.Leader, payment, false);
            MakePeaceAction.Apply(kingdom, faction);
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

        private static void CreatePeaceInqiry(Kingdom kingdom, IFaction faction, int payment)
        {
            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=BkGSVccZ}Peace Proposal").ToString(), CreateMakePeaceInquiryText(kingdom, faction, payment), true, true, new TextObject("{=Y94H6XnK}Accept").ToString(), new TextObject("{=cOgmdp9e}Decline").ToString(), () =>
            {
                KingdomPeaceAction.AcceptPeace(kingdom, faction, payment);
            }, null, ""), true);
        }


        public static void ApplyPeace(Kingdom kingdom, IFaction faction, int num2)
        {
            if (PlayerHelpers.IsPlayerLeaderOfFaction(faction))
            {
                KingdomPeaceAction.CreatePeaceInqiry(kingdom, faction, num2);
            }
            else
            {
                KingdomPeaceAction.AcceptPeace(kingdom, faction, num2);
            }
        }
    }
}
