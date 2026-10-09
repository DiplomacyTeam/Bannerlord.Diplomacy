using Diplomacy.CivilWar.Factions;

using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace Diplomacy.CivilWar
{
    internal sealed class FactionRecruitmentRecord
    {
        [SaveableField(1)] private readonly Clan _clan;
        [SaveableField(2)] private readonly Hero _leader;
        [SaveableField(3)] private readonly RebelFaction _faction;
        [SaveableField(4)] private readonly CampaignTime _cooldownUntil;
        [SaveableField(5)] private CampaignTime _pledgeUntil;
        [SaveableField(6)] private bool _resolved;
        [SaveableField(7)] private readonly float _bonus;
        [SaveableField(8)] private readonly float _pledgeDuration;

        public Clan Clan => _clan;
        public Hero Leader => _leader;
        public RebelFaction Faction => _faction;
        public CampaignTime CooldownUntil => _cooldownUntil;
        public CampaignTime PledgeUntil => _pledgeUntil;
        public float Bonus => _bonus;
        public bool Resolved => _resolved;

        public FactionRecruitmentRecord(Clan clan, RebelFaction faction, float bonus, float cooldownDays, float pledgeDays)
        {
            _clan = clan;
            _leader = clan.Leader;
            _faction = faction;
            _bonus = bonus;
            _cooldownUntil = CampaignTime.DaysFromNow(cooldownDays);
            _pledgeDuration = pledgeDays;
        }

        public void Resolve(bool pledged)
        {
            _resolved = true;
            if (pledged)
                _pledgeUntil = CampaignTime.DaysFromNow(_pledgeDuration);
        }

        public void EndPledge() => _pledgeUntil = CampaignTime.Zero;
    }
}