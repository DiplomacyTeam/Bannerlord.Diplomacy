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

        public Dictionary<FactionMapping, List<DiplomaticAgreement>> Agreements { get { return _agreements; } }

        public bool HasNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom, out NonAggressionPactAgreement pactAgreement)
        {
            if (this._agreements.TryGetValue(new FactionMapping(kingdom, otherKingdom), out List<DiplomaticAgreement> agreements))
            {
                IEnumerable<DiplomaticAgreement> enumerable = agreements.Where(agreement => agreement.GetAgreementType() == AgreementType.NonAggressionPact && !agreement.IsExpired());
                pactAgreement = enumerable.OfType<NonAggressionPactAgreement>().FirstOrDefault();
                return enumerable.Any();
            }
            else
            {
                pactAgreement = null;
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
