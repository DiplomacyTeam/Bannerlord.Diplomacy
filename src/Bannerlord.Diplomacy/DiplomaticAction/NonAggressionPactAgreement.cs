using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction
{
    class NonAggressionPactAgreement : DiplomaticAgreement
    {
        public NonAggressionPactAgreement(CampaignTime startdate, CampaignTime endDate, Kingdom kingdom, Kingdom otherKingdom)
            : base(startdate, endDate, kingdom, otherKingdom) { }

        public override AgreementType GetAgreementType() => AgreementType.NonAggressionPact;

        public override void NotifyExpired()
        {
            var txt = new TextObject("{=uWY09LJb}The non-aggression pact between {KINGDOM} and {OTHER_KINGDOM} has expired.");
            txt.SetTextVariable("KINGDOM", Factions.Faction1.MapFaction.Name);
            txt.SetTextVariable("OTHER_KINGDOM", Factions.Faction2.MapFaction.Name);
            var txtRendered = txt.ToString();

            if (Factions.Faction1.MapFaction == Clan.PlayerClan.Kingdom || Factions.Faction2.MapFaction == Clan.PlayerClan.Kingdom)
            {
                InformationManager.ShowInquiry(
                    new InquiryData(
                        new TextObject("{=pdI7Tjtj}Non-Aggression Pact Expired").ToString(),
                        txtRendered,
                        true,
                        false,
                        GameTexts.FindText("str_ok").ToString(),
                        null,
                        null,
                        null), true);
            }
            else
                InformationManager.DisplayMessage(new InformationMessage(txtRendered));
        }
    }
}