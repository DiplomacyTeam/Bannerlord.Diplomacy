using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace Diplomacy
{
    /// <summary>
    /// Tracks who owned each fief before its current holder. The game itself keeps no ownership
    /// history, and the war exhaustion event records that could stand in for one are discarded the
    /// moment peace is made, so returning a fief taken in an earlier war needs its own store.
    /// </summary>
    internal sealed class FiefProvenanceManager
    {
        private const int MaxRecordsPerFief = 10;

        private static readonly IReadOnlyList<FiefOwnershipRecord> _noHistory = Array.Empty<FiefOwnershipRecord>();

        [SaveableField(1)][UsedImplicitly] private Dictionary<string, List<FiefOwnershipRecord>> _ownershipHistory;

        public static FiefProvenanceManager? Instance { get; private set; }

        public FiefProvenanceManager()
        {
            _ownershipHistory = new Dictionary<string, List<FiefOwnershipRecord>>();
            Instance = this;
        }

        /// <summary>Records a change of ownership, newest first. Only towns and castles are tracked.</summary>
        public void RegisterTransfer(Settlement settlement, Hero? oldOwner, Hero? newOwner, bool byConquest)
        {
            AddRecord(settlement, new FiefOwnershipRecord(oldOwner?.MapFaction as Kingdom,
                                                          oldOwner?.Clan,
                                                          newOwner?.MapFaction as Kingdom,
                                                          newOwner?.Clan,
                                                          CampaignTime.Now,
                                                          byConquest));
        }

        /// <summary>
        /// Records a clan carrying its fiefs into another kingdom. The owner doesn't change, so the game
        /// raises no settlement event, but the fiefs now belong to a kingdom that didn't conquer them.
        /// Without this, a fief taken as another kingdom's vassal could be returned for payouts that the
        /// clan's new kingdom never earned.
        /// </summary>
        public void RegisterKingdomChange(Clan clan, Kingdom? oldKingdom, Kingdom? newKingdom)
        {
            if (oldKingdom == newKingdom)
                return;

            foreach (var town in clan.Fiefs.ToList())
                AddRecord(town.Settlement, new FiefOwnershipRecord(oldKingdom, clan, newKingdom, clan, CampaignTime.Now, false));
        }

        private void AddRecord(Settlement settlement, FiefOwnershipRecord record)
        {
            if (!settlement.IsTown && !settlement.IsCastle)
                return;

            if (!_ownershipHistory.TryGetValue(settlement.StringId, out var history))
                _ownershipHistory[settlement.StringId] = history = new List<FiefOwnershipRecord>();

            history.Insert(0, record);

            if (history.Count > MaxRecordsPerFief)
                history.RemoveRange(MaxRecordsPerFief, history.Count - MaxRecordsPerFief);
        }

        public IReadOnlyList<FiefOwnershipRecord> GetHistory(Settlement settlement) =>
            _ownershipHistory.TryGetValue(settlement.StringId, out var history) ? history : _noHistory;

        /// <summary>
        /// The transfer that brought this fief into its current kingdom. Grants and inheritances
        /// inside a single kingdom are skipped, so provenance survives handing a conquered fief to
        /// a vassal.
        /// </summary>
        public FiefOwnershipRecord? GetLastCrossKingdomTransfer(Settlement settlement) =>
            GetHistory(settlement).FirstOrDefault(record => record.IsCrossKingdom);

        /// <summary>Whether the fief's current kingdom took it from <paramref name="kingdom"/> by force.</summary>
        public bool WasConqueredFrom(Settlement settlement, Kingdom kingdom, out FiefOwnershipRecord? transfer)
        {
            transfer = GetLastCrossKingdomTransfer(settlement);
            return transfer is not null && transfer.ByConquest && transfer.PreviousKingdom == kingdom;
        }

        /// <summary>
        /// The clan that lost the fief when <paramref name="kingdom"/> lost it, provided that clan
        /// still exists, still has a leader and is still a vassal of the same kingdom. Returning a
        /// fief to its old holder is worth more than handing it to whoever happens to wear the crown.
        /// </summary>
        public Clan? GetDispossessedClan(Settlement settlement, Kingdom kingdom)
        {
            if (!WasConqueredFrom(settlement, kingdom, out var transfer))
                return null;

            // Clan.Kingdom is also set for mercenaries, who must not be handed fiefs (see GrantFiefAction).
            var clan = transfer!.PreviousOwnerClan;
            return clan is not null
                   && !clan.IsEliminated
                   && clan.Leader is not null
                   && !clan.IsUnderMercenaryService
                   && !clan.IsMinorFaction
                   && clan.Kingdom == kingdom
                ? clan
                : null;
        }

        /// <summary>How often the fief has passed between kingdoms, used to damp repeat payouts.</summary>
        public int GetTimesChangedHands(Settlement settlement) =>
            GetHistory(settlement).Count(record => record.IsCrossKingdom);

        /// <summary>When the fief's current kingdom took possession of it.</summary>
        public CampaignTime GetHeldByCurrentKingdomSince(Settlement settlement) =>
            GetLastCrossKingdomTransfer(settlement)?.TransferDate ?? CampaignTime.Zero;

        /// <summary>
        /// Moves the newest record for a fief further into the past. Exists for the debug cheats,
        /// so that a freshly seized fief can be made to look long-held.
        /// </summary>
        internal bool BackdateLastTransfer(Settlement settlement, float days)
        {
            if (!_ownershipHistory.TryGetValue(settlement.StringId, out var history) || history.Count == 0)
                return false;

            var record = history[0];
            history[0] = new FiefOwnershipRecord(record.PreviousKingdom,
                                                 record.PreviousOwnerClan,
                                                 record.NewKingdom,
                                                 record.NewOwnerClan,
                                                 CampaignTime.DaysFromNow(-days),
                                                 record.ByConquest);
            return true;
        }

        internal void Sync()
        {
            _ownershipHistory ??= new Dictionary<string, List<FiefOwnershipRecord>>();
            Instance = this;
        }
    }
}
