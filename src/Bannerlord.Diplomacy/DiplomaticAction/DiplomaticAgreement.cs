using Microsoft.Extensions.Logging;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace Diplomacy.DiplomaticAction
{
    abstract class DiplomaticAgreement
    {
        [SaveableProperty(1)]
        public CampaignTime StartDate { get; protected set; }

        [SaveableProperty(2)]
        public CampaignTime EndDate { get; protected set; }

        [SaveableProperty(3)]
        public FactionPair Factions { get; set; }

        [SaveableProperty(4)]
        public bool ExpireNotified { get; protected set; }

        protected DiplomaticAgreement(CampaignTime startdate, CampaignTime endDate, Kingdom kingdom, Kingdom otherKingdom)
            : this(startdate, endDate, new FactionPair(kingdom, otherKingdom)) { }

        protected DiplomaticAgreement(CampaignTime startdate, CampaignTime endDate, FactionPair factionMapping)
        {
            StartDate = startdate;
            EndDate = endDate;
            Factions = factionMapping;
        }

        public abstract AgreementType GetAgreementType();

        public virtual void Expire()
        {
            EndDate = CampaignTime.Now - CampaignTime.Milliseconds(1);
            TryExpireNotification();
        }

        public void TryExpireNotification()
        {
            if (!ExpireNotified && IsExpired())
            {
                LogFactory.Get<DiplomaticAgreement>()
                    .LogTrace($"[{CampaignTime.Now}] Agreement expired between {Factions.Faction1.Name} and {Factions.Faction2.Name}:"
                            + $" {Enum.GetName(typeof(AgreementType), GetAgreementType())}");
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