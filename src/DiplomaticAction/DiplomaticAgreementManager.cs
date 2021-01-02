using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace Diplomacy.DiplomaticAction
{
    [SaveableClass(7)]
    class DiplomaticAgreementManager
    {
        public static DiplomaticAgreementManager Instance { get; internal set; }

        [SaveableField(1)]
        private Dictionary<FactionPair, List<DiplomaticAgreement>> _agreements;

        public DiplomaticAgreementManager()
        {
            _agreements = new Dictionary<FactionPair, List<DiplomaticAgreement>>();
            Instance = this;
        }

        public Dictionary<FactionPair, List<DiplomaticAgreement>> Agreements => _agreements;

        public bool HasNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom, out NonAggressionPactAgreement pactAgreement)
        {
            if (_agreements.TryGetValue(new FactionPair(kingdom, otherKingdom), out var agreements))
            {
                var enumerable = agreements.Where(agreement => agreement.GetAgreementType() == AgreementType.NonAggressionPact && !agreement.IsExpired());
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
            var factionMapping = new FactionPair(kingdom, otherKingdom);
            if (_agreements.TryGetValue(factionMapping, out var agreements))
            {
                agreements.Add(diplomaticAgreement);
            }
            else
            {
                _agreements[factionMapping] = new List<DiplomaticAgreement>() { diplomaticAgreement };
            }
        }
        public void Sync()
        {
            Instance = this;
        }
    }
}
