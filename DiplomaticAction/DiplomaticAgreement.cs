
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes.DiplomaticAction
{
    [SaveableClass(5)]
    abstract class DiplomaticAgreement
    {
        [SaveableProperty(1)]
        public CampaignTime StartDate { get; protected set; }
        [SaveableProperty(2)]
        public CampaignTime EndDate { get; protected set; }
        
        public DiplomaticAgreement(CampaignTime startdate, CampaignTime endDate)
        {
            this.StartDate = startdate;
            this.EndDate = endDate;
        }

        public abstract AgreementType GetAgreementType();
        public abstract void Expire();
    }

    public enum AgreementType
    {
        NonAggressionPact
    }
}
