using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes.DiplomaticAction
{
    [SaveableClass(6)]
    class NonAggressionPactAgreement : DiplomaticAgreement
    {
        public NonAggressionPactAgreement(CampaignTime startdate, CampaignTime endDate) : base(startdate, endDate)
        {
        }

        public override void Expire()
        {
            this.EndDate = CampaignTime.Now - CampaignTime.Milliseconds(1);
        }

        public override AgreementType GetAgreementType()
        {
            return AgreementType.NonAggressionPact;
        }
    }
}
