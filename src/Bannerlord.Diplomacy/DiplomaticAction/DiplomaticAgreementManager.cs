using JetBrains.Annotations;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace Diplomacy.DiplomaticAction
{
    class DiplomaticAgreementManager
    {
        public static DiplomaticAgreementManager? Instance { get; internal set; }

        [SaveableField(1)]
        [UsedImplicitly]
        private Dictionary<FactionPair, List<DiplomaticAgreement>> _agreements;

        public DiplomaticAgreementManager()
        {
            _agreements = new Dictionary<FactionPair, List<DiplomaticAgreement>>();
            Instance = this;
        }

        public Dictionary<FactionPair, List<DiplomaticAgreement>> Agreements => _agreements;

        public static bool HasNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom, out NonAggressionPactAgreement? pactAgreement)
        {
            if (Instance!.Agreements.TryGetValue(new FactionPair(kingdom, otherKingdom), out var agreements))
            {
                var enumerable = agreements.Where(agreement => agreement.GetAgreementType() == AgreementType.NonAggressionPact && !agreement.IsExpired());
                pactAgreement = enumerable.OfType<NonAggressionPactAgreement>().FirstOrDefault();
                return pactAgreement != default;
            }
            else
            {
                pactAgreement = null;
                return false;
            }
        }

        public static void RegisterAgreement(Kingdom kingdom, Kingdom otherKingdom, DiplomaticAgreement diplomaticAgreement)
        {
            var factionMapping = new FactionPair(kingdom, otherKingdom);
            if (Instance!.Agreements.TryGetValue(factionMapping, out var agreements))
            {
                agreements.Add(diplomaticAgreement);
            }
            else
            {
                Instance!.Agreements[factionMapping] = new List<DiplomaticAgreement> { diplomaticAgreement };
            }
        }
        /// <summary>
        /// Announces agreements that have just expired and forgets the ones that are over. Nothing reads an
        /// agreement once it has expired, and keeping them grows the save with every pact ever formed.
        /// </summary>
        internal void UpdateAgreements()
        {
            foreach (var factions in _agreements.Keys.ToList())
            {
                var agreements = _agreements[factions];

                // A pact with a kingdom that no longer exists is dropped without announcing its expiry.
                if (factions.Faction1 is null or { IsEliminated: true } || factions.Faction2 is null or { IsEliminated: true })
                {
                    _agreements.Remove(factions);
                    continue;
                }

                foreach (var agreement in agreements.ToList())
                {
                    agreement.TryExpireNotification();
                    if (agreement.ExpireNotified)
                        agreements.Remove(agreement);
                }

                if (agreements.Count == 0)
                    _agreements.Remove(factions);
            }
        }

        public void Sync()
        {
            Instance = this;
        }
    }
}