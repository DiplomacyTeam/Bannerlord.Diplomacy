using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace DiplomacyFixes
{
    class KeepFiefHelper
    {
        public static void ShowKeepFiefInquiry(Settlement settlement, Action declineAction)
        {
            InformationManager.ShowInquiry(
                new InquiryData(
                    new TextObject("{=N06wk0dB}Settlement Captured").ToString(),
                    GetKeepFiefText(settlement).ToString(),
                    true, true,
                    new TextObject("{=Y94H6XnK}Accept").ToString(),
                    new TextObject("{=cOgmdp9e}Decline").ToString(), delegate ()
                    {
                        ChangeOwnerOfSettlementAction.ApplyByDefault(Hero.MainHero, settlement);
                    }, declineAction, ""), true);
        }

        private static TextObject GetKeepFiefText(Settlement settlement)
        {
            TextObject textObject = new TextObject("{=Zy0yjTha}As the capturer of {SETTLEMENT_NAME}, you have the right of first refusal. Would you like to claim this fief?");
            textObject.SetTextVariable("SETTLEMENT_NAME", settlement.Name);

            return textObject;
        }
    }
}
