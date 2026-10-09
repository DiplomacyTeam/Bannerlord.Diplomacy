using Diplomacy.CivilWar.Actions;
using Diplomacy.CivilWar.Factions;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace Diplomacy.CivilWar
{
    internal sealed class FactionRecruitmentManager
    {
        [SaveableField(1)] private List<FactionRecruitmentRecord> _records = new();

        public static FactionRecruitmentManager? Instance { get; private set; }

        public FactionRecruitmentManager() => Instance = this;

        public void Sync()
        {
            Instance = this;
            _records ??= new();
            // Conversations are not resumed on load. An interrupted attempt still consumes its cooldown.
            foreach (var record in _records.Where(r => !r.Resolved))
                record.Resolve(false);
        }

        public int GetCooldownDaysRemaining(Clan clan)
        {
            var record = _records.FirstOrDefault(r => r.Clan == clan);
            return record is null ? 0 : Math.Max(0, (int) Math.Ceiling(-record.CooldownUntil.ElapsedDaysUntilNow));
        }

        public FactionRecruitmentRecord StartAttempt(Clan clan, RebelFaction faction, float bonus, float cooldownDays, float pledgeDays)
        {
            _records.RemoveAll(r => r.Clan == clan);
            var record = new FactionRecruitmentRecord(clan, faction, bonus, cooldownDays, pledgeDays);
            _records.Add(record);
            return record;
        }

        public bool IsPendingAttempt(FactionRecruitmentRecord record)
            => !record.Resolved && _records.Contains(record);

        public bool HasPendingAttempt(Clan clan) => _records.Any(r => r.Clan == clan && !r.Resolved);

        public bool HasActivePledge(Clan clan, RebelFaction faction) => GetPledgedFaction(clan) == faction;

        // A clan has at most one record, as each new attempt replaces the previous one.
        public RebelFaction? GetPledgedFaction(Clan clan)
        {
            var record = _records.FirstOrDefault(r => r.Clan == clan);
            return record is not null && record.PledgeUntil.ElapsedDaysUntilNow < 0f && IsPledgeValid(record) ? record.Faction : null;
        }

        public int GetPledgeDaysRemaining(Clan clan, RebelFaction faction)
        {
            if (!HasActivePledge(clan, faction))
                return 0;
            return (int) Math.Ceiling(-_records.First(r => r.Clan == clan && r.Faction == faction).PledgeUntil.ElapsedDaysUntilNow);
        }

        public void Cleanup()
        {
            foreach (var record in _records.Where(r => !IsPledgeValid(r)))
                record.EndPledge();
            _records.RemoveAll(r => r.Resolved && r.CooldownUntil.ElapsedDaysUntilNow >= 0f && r.PledgeUntil.ElapsedDaysUntilNow >= 0f);
        }

        private static bool IsPledgeValid(FactionRecruitmentRecord record)
        {
            var clan = record.Clan;
            var faction = record.Faction;
            return !faction.ParentKingdom.IsEliminated && clan.Leader == record.Leader && !clan.IsMinorFaction
                   && faction.Clans.Contains(clan) && faction.SponsorClan == Clan.PlayerClan
                   && RebelFactionManager.GetRebelFaction(faction.ParentKingdom).Contains(faction)
                   && JoinFactionAction.IsEligibleMember(clan, faction);
        }
    }
}