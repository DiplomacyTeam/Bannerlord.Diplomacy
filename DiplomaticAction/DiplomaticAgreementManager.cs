using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes.DiplomaticAction
{
    [SaveableClass(7)]
    class DiplomaticAgreementManager
    {
        public static DiplomaticAgreementManager Instance { get; internal set; }

        [SaveableField(1)]
        private Dictionary<FactionMapping, List<DiplomaticAgreement>> _agreements;

        public DiplomaticAgreementManager()
        {
            this._agreements = new Dictionary<FactionMapping, List<DiplomaticAgreement>>();
            Instance = this;
        }

        public bool HasNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom) 
        {
            if (this._agreements.TryGetValue(new FactionMapping(kingdom, otherKingdom), out List<DiplomaticAgreement> agreements))
            {
                return agreements.Where(agreement => agreement.GetAgreementType() == AgreementType.NonAggressionPact && CampaignTime.Now <= agreement.EndDate).Any();
            }
            else
            {
                return false;
            }
        }

        public void RegisterAgreement(Kingdom kingdom, Kingdom otherKingdom, DiplomaticAgreement diplomaticAgreement)
        {
            FactionMapping factionMapping = new FactionMapping(kingdom, otherKingdom);
            if (this._agreements.TryGetValue(factionMapping, out List<DiplomaticAgreement> agreements))
            {
                agreements.Add(diplomaticAgreement);
            }
            else
            {
                this._agreements[factionMapping] = new List<DiplomaticAgreement>() { diplomaticAgreement };
            }
        }

        public void Sync()
        {
            Instance = this;
        }
    }
}
