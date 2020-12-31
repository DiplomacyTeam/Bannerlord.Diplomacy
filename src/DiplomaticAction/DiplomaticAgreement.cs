
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace Diplomacy.DiplomaticAction
{
    [SaveableClass(5)]
    abstract class DiplomaticAgreement
    {
        [SaveableProperty(1)]
        public CampaignTime StartDate { get; protected set; }
        [SaveableProperty(2)]
        public CampaignTime EndDate { get; protected set; }
        [SaveableProperty(3)]
        public FactionMapping Factions { get; set; }
        [SaveableProperty(4)]
        public bool ExpireNotified { get; protected set; }

        public DiplomaticAgreement(CampaignTime startdate, CampaignTime endDate, Kingdom kingdom, Kingdom otherKingdom) : this(startdate, endDate, new FactionMapping(kingdom, otherKingdom)) { }

        public DiplomaticAgreement(CampaignTime startdate, CampaignTime endDate, FactionMapping factionMapping)
        {
            this.StartDate = startdate;
            this.EndDate = endDate;
            this.Factions = factionMapping;
        }

        public abstract AgreementType GetAgreementType();
        public virtual void Expire()
        {
            this.EndDate = CampaignTime.Now - CampaignTime.Milliseconds(1);
            this.TryExpireNotification();
        }
        public void TryExpireNotification()
        {
            if (!ExpireNotified && IsExpired())
            {
                NotifyExpired();
                ExpireNotified = true;
            }
        }
        public abstract void NotifyExpired();

        public bool IsExpired()
        {
            return EndDate.IsPast;
        }
    }

    public enum AgreementType
    {
        NonAggressionPact
    }
}
