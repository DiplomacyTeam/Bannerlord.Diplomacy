using Microsoft.Extensions.Logging;

using TaleWorlds.CampaignSystem;

namespace Diplomacy.CampaignBehaviors
{
    /// <summary>
    /// Keeps every kingdom clan's <see cref="Clan.FactionMidSettlement"/> set.
    /// </summary>
    /// <remarks>
    /// Vanilla values a fief for a clan through <c>DefaultSettlementValueModel.GeographicalAdvantageForFaction</c>,
    /// which reads the clan's mid-settlement without a null check. That value is a cache, recalculated only when
    /// the game loads or the clan gains or loses a fief. A clan without fiefs uses its home settlement instead, so
    /// a clan that had no home at the last recalculation keeps a null mid-settlement. Joining a kingdom gives it a
    /// home but leaves the cache null, and the next settlement claimant vote or corruption fief sale in that
    /// kingdom throws a NullReferenceException.
    /// </remarks>
    internal sealed class MidSettlementRepairBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, RepairAll);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, (clan, _, newKingdom, _, _) =>
            {
                if (newKingdom is not null)
                    EnsureMidSettlement(clan);
            });
        }

        public override void SyncData(IDataStore dataStore) { }

        private static void RepairAll()
        {
            foreach (var kingdom in Kingdom.All)
                foreach (var clan in kingdom.Clans)
                    EnsureMidSettlement(clan);
        }

        /// <summary>
        /// Recalculates <paramref name="clan"/>'s mid-settlement if it is missing, finding the clan a home
        /// settlement first when it has none.
        /// </summary>
        /// <returns><see langword="true"/> if fiefs can be valued for <paramref name="clan"/> afterwards.</returns>
        public static bool EnsureMidSettlement(Clan clan)
        {
            if (clan.IsEliminated || clan.FactionMidSettlement is not null)
                return true;

            if (clan.HomeSettlement is null)
                clan.ConsiderAndUpdateHomeSettlement();

            clan.CalculateMidSettlement();

            if (clan.FactionMidSettlement is not null)
                return true;

            LogFactory.Get<MidSettlementRepairBehavior>()
                .LogWarning($"{clan.Name} of {clan.Kingdom?.Name} has no settlement to value fiefs from.");
            return false;
        }
    }
}
