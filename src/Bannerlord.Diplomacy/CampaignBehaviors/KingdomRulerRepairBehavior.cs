using Microsoft.Extensions.Logging;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Diplomacy.CampaignBehaviors
{
    /// <summary>
    /// Keeps every kingdom's throne occupied.
    /// </summary>
    /// <remarks>
    /// Vanilla treats <see cref="Kingdom.Leader"/> (which is just <c>RulingClan?.Leader</c>) as if it
    /// could never be null and dereferences it all over the daily tick: barter evaluation, decision
    /// proposal, <see cref="Kingdom.AddDecision"/>, the diplomacy model. The state it can't handle is
    /// real, though: <see cref="KillCharacterAction"/> can leave a kingdom with a null ruling clan (no
    /// eligible successor clan) or with a ruling clan whose own leader died without an heir, and
    /// there is no self-healing pass in vanilla outside a pre-e1.8.0 save migration. Civil wars and
    /// abdications make it far more likely, so Diplomacy repairs it instead of guarding every reader.
    /// </remarks>
    internal sealed class KingdomRulerRepairBehavior : CampaignBehaviorBase
    {
        private static bool _repairing;

        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, RepairAll);
            CampaignEvents.RulingClanChanged.AddNonSerializedListener(this, OnRulingClanChanged);

            // Every way a throne comes to be vacant. The victim's own clan is deliberately not used as
            // the starting point on a death: DestroyClanAction.ApplyByClanLeaderDeath runs before
            // HeroKilledEvent is dispatched, so by then the clan has already left the kingdom it ruled.
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, (_, _, _, _) => RepairAll());
            CampaignEvents.OnClanDestroyedEvent.AddNonSerializedListener(this, _ => RepairAll());
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, (_, _, _, _, _) => RepairAll());

            // Backstop: clan daily ticks are spread across the whole day, so a throne vacated by some
            // path not listed above must not stay vacant until tomorrow.
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, RepairAll);
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnRulingClanChanged(Kingdom kingdom, Clan oldRulingClan) => EnsureRuler(kingdom);

        private void RepairAll()
        {
            foreach (var kingdom in Kingdom.All.ToList())
                EnsureRuler(kingdom);
        }

        /// <summary>
        /// Makes sure <paramref name="kingdom"/> has a living leader, promoting an heir within the
        /// ruling clan or handing the throne to another clan if it doesn't.
        /// </summary>
        /// <returns><see langword="true"/> if the kingdom has a living leader afterwards.</returns>
        public static bool EnsureRuler(Kingdom? kingdom)
        {
            if (kingdom is null || kingdom.IsEliminated)
                return false;

            if (kingdom.Leader is { IsAlive: true })
                return true;

            // ChangeRulingClanAction fires RulingClanChanged, which lands right back here.
            if (_repairing)
                return false;

            _repairing = true;

            try
            {
                // Preferred: keep the dynasty, promote someone from the ruling clan. ChangeClanLeaderAction
                // is unusable here because it reads the (null) outgoing leader's gold first.
                if (kingdom.RulingClan is { IsEliminated: false } rulingClan && FindHeir(rulingClan) is { } heir)
                {
                    rulingClan.SetLeader(heir);
                    LogFactory.Get<KingdomRulerRepairBehavior>()
                        .LogInformation($"{kingdom.Name} had no leader; promoted {heir.Name} to lead the ruling clan {rulingClan.Name}.");
                    return true;
                }

                if (FindEligibleRulingClan(kingdom) is { } replacement)
                {
                    ChangeRulingClanAction.Apply(kingdom, replacement);
                    LogFactory.Get<KingdomRulerRepairBehavior>()
                        .LogInformation($"{kingdom.Name} had no leader; the throne passed to {replacement.Name}.");
                    return true;
                }

                // Nothing left to crown. Vanilla destroys the kingdom in this situation; leave that
                // call to it and let the patches keep the tick alive until it happens.
                LogFactory.Get<KingdomRulerRepairBehavior>()
                    .LogWarning($"{kingdom.Name} has no leader and no clan able to take the throne.");
                return false;
            }
            finally
            {
                _repairing = false;
            }
        }

        private static Hero? FindHeir(Clan clan) => clan.Heroes
            .Where(h => h.IsAlive && h.IsLord && !h.IsChild && !h.IsDisabled && !h.IsNotSpawned)
            .OrderByDescending(h => h.Age)
            .FirstOrDefault();

        private static Clan? FindEligibleRulingClan(Kingdom kingdom) => kingdom.Clans
            .Where(c => c != kingdom.RulingClan && !c.IsEliminated && !c.IsUnderMercenaryService && c.Leader is { IsAlive: true })
            .OrderByDescending(c => c.CurrentTotalStrength)
            .FirstOrDefault();
    }
}
