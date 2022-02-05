using JetBrains.Annotations;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction
{
    internal sealed class TributeAgreement : DiplomaticAgreement
    {
        public int PaymentAmount { get; }

        // payments made, payments received
        //public Tuple<int, int> PaymentsRemaining { get; }

        public int PaymentsMade { get; set; }
        public int PaymentsReceived { get; set; }
        public int TotalPayments { get; }

        public IFaction Payer { get; }
        public IFaction Payee { get; }


        public TributeAgreement(CampaignTime startdate, CampaignTime endDate, [NotNull] Kingdom kingdom, [NotNull] Kingdom otherKingdom, int paymentAmount, int paymentsRemaining) : base(startdate, endDate, kingdom, otherKingdom)
        {
            PaymentAmount = paymentAmount;
            TotalPayments = paymentsRemaining;
            Payer = kingdom;
            Payee = otherKingdom;
        }

        public override AgreementType GetAgreementType()
        {
            return AgreementType.Tribute;
        }

        public void RegisterPayment(ref ExplainedNumber explainedNumber, IFaction faction, bool applyPayment)
        {
            if (faction == Payer && PaymentsMade < TotalPayments)
            {
                explainedNumber.Add(-PaymentAmount, new TextObject("{=}Tribute to {KINGDOM} ({PAYMENTS_REMAINING})", new() { { "KINGDOM", Payee.Name }, { "PAYMENTS_REMAINING", TotalPayments - PaymentsMade } }));
                if(applyPayment)
                    ++PaymentsMade;
            }
            else if (PaymentsReceived < TotalPayments)
            {
                explainedNumber.Add(PaymentAmount, new TextObject("{=}Tribute from {KINGDOM} ({PAYMENTS_REMAINING})", new() { { "KINGDOM", Payer.Name }, {"PAYMENTS_REMAINING", TotalPayments - PaymentsReceived} }));
                if (applyPayment)
                    ++PaymentsReceived;
            }

            if (TotalPayments == PaymentsMade && TotalPayments == PaymentsReceived)
            {
                Expire();
            }
        }

        public override void NotifyExpired()
        {
            var txt = new TextObject("{=}The tribute between {KINGDOM} and {OTHER_KINGDOM} has been paid in full.");
            txt.SetTextVariable("KINGDOM", Factions.Faction1.MapFaction.Name);
            txt.SetTextVariable("OTHER_KINGDOM", Factions.Faction2.MapFaction.Name);
            var txtRendered = txt.ToString();

            if (Factions.Faction1.MapFaction == Clan.PlayerClan.Kingdom || Factions.Faction2.MapFaction == Clan.PlayerClan.Kingdom)
            {
                InformationManager.ShowInquiry(
                    new InquiryData(
                        new TextObject("{=}Tribute Paid in Full").ToString(),
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
