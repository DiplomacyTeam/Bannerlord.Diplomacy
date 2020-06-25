using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes.DiplomaticAction
{
    [SaveableClass(6)]
    class NonAggressionPactAgreement : DiplomaticAgreement
    {
        public NonAggressionPactAgreement(CampaignTime startdate, CampaignTime endDate, Kingdom kingdom, Kingdom otherKingdom) : base(startdate, endDate, kingdom, otherKingdom)
        {
        }

        public override AgreementType GetAgreementType()
        {
            return AgreementType.NonAggressionPact;
        }

        public override void NotifyExpired()
        {
            TextObject textObject = new TextObject("{=uWY09LJb}The non-aggression pact between {KINGDOM} and {OTHER_KINGDOM} has expired.");
            textObject.SetTextVariable("KINGDOM", Factions.Faction1.MapFaction.Name);
            textObject.SetTextVariable("OTHER_KINGDOM", Factions.Faction2.MapFaction.Name);
            if (Factions.Faction1.MapFaction == Clan.PlayerClan.Kingdom || Factions.Faction2.MapFaction == Clan.PlayerClan.Kingdom)
            {

                InformationManager.ShowInquiry(
                    new InquiryData(
                        new TextObject("{=pdI7Tjtj}Non-Aggression Pact Expired").ToString(),
                        textObject.ToString(),
                        true,
                        false,
                        GameTexts.FindText("str_ok", null).ToString(),
                        null,
                        null,
                        null,
                        ""), true);
            } 
            else
            {
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
            }
        }
    }
}
