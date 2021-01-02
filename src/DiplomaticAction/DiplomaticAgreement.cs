using Microsoft.Extensions.Logging;

using System;

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

        public DiplomaticAgreement(CampaignTime startdate, CampaignTime endDate, Kingdom kingdom, Kingdom otherKingdom)
            : this(startdate, endDate, new FactionMapping(kingdom, otherKingdom)) { }

        public DiplomaticAgreement(CampaignTime startdate, CampaignTime endDate, FactionMapping factionMapping)
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
                Log.Get<DiplomaticAgreement>()
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
