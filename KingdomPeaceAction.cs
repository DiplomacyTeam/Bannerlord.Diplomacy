using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

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
            List<string> inquiryText = new List<string>();
            inquiryText.AddRange(new string[]
            {
                kingdom.Leader.Name.ToString(),
                " of the ",
                kingdom.Name.ToString(),
                " is proposing peace with the ",
                faction.Name.ToString(),
                "."
            });

            if (payment > 0)
            {
                inquiryText.AddRange(new string[] 
                {
                    "They are willing to pay war reparations of ",
                    payment.ToString(),
                    " denars."
                });
            }
            return String.Concat(inquiryText);
        }

        private static void CreatePeaceInqiry(Kingdom kingdom, IFaction faction, int payment)
        {
            InformationManager.ShowInquiry(new InquiryData("Peace Proposal", CreateMakePeaceInquiryText(kingdom, faction, payment), true, true, "Accept", "Decline", () =>
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
