using Diplomacy.DiplomaticAction.NonAggressionPact;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace Diplomacy
{
    [SaveableClass(3)]
    class CooldownManager
    {
        internal static Dictionary<string, CampaignTime> LastWarTime { get; private set; }
        internal static Dictionary<Kingdom, CampaignTime> LastPeaceProposalTime { get; private set; }
        internal static Dictionary<string, CampaignTime> LastAllianceFormedTime { get; private set; }

        [SaveableField(1)]
        private Dictionary<string, CampaignTime> _lastWarTime;

        [SaveableField(2)]
        private Dictionary<Kingdom, CampaignTime> _lastPeaceProposalTime;

        [SaveableField(3)]
        private Dictionary<string, CampaignTime> _lastAllianceFormedTime;

        private static float MinimumDaysBetweenPeaceProposals => 5f;

        internal CooldownManager()
        {
            _lastWarTime = new Dictionary<string, CampaignTime>();
            _lastPeaceProposalTime = new Dictionary<Kingdom, CampaignTime>();
            _lastAllianceFormedTime = new Dictionary<string, CampaignTime>();
            LastWarTime = _lastWarTime;
            LastPeaceProposalTime = _lastPeaceProposalTime;
            LastAllianceFormedTime = _lastAllianceFormedTime;
        }

        public void UpdateLastPeaceProposalTime(Kingdom kingdom, CampaignTime campaignTime)
        {
            _lastPeaceProposalTime[kingdom] = campaignTime;
        }

        public void UpdateLastAllianceFormedTime(Kingdom kingdom1, Kingdom kingdom2, CampaignTime campaignTime)
        {
            var key = CreateKey(kingdom1, kingdom2);
            _lastAllianceFormedTime[key] = campaignTime;
        }

        public static bool HasBreakAllianceCooldown(Kingdom kingdom1, Kingdom kingdom2, out float elapsedDaysUntilNow)
        {
            var hasBreakAllianceCooldown = false;
            if (LastAllianceFormedTime.TryGetValue(CreateKey(kingdom1, kingdom2), out var value))
            {
                hasBreakAllianceCooldown = (elapsedDaysUntilNow = value.ElapsedDaysUntilNow) < Settings.Instance.MinimumAllianceDuration;
            }
            else
            {
                elapsedDaysUntilNow = default;
            }

            return hasBreakAllianceCooldown;
        }

        private static string CreateKey(IFaction faction1, IFaction faction2)
        {
            var factions = new List<IFaction> { faction1, faction2 };
            var keyArguments = factions.OrderBy(faction => faction.StringId).Select(faction => faction.StringId);
            return string.Join("+", keyArguments);
        }

        private static bool DecodeKeyToKingdoms(string key, out Tuple<Kingdom, Kingdom> kingdoms)
        {
            Kingdom faction1, faction2;

            var factionStringIds = key.Split('+');
            faction1 = MBObjectManager.Instance.GetObject<Kingdom>(factionStringIds[0]);
            faction2 = MBObjectManager.Instance.GetObject<Kingdom>(factionStringIds[1]);

            if (faction1 is not null && faction2 is not null)
            {
                kingdoms = Tuple.Create(faction1, faction2);
                return true;
            }
            else
            {
                kingdoms = default;
                return false;
            }
        }

        public static CampaignTime? GetLastWarTimeBetweenFactions(IFaction faction1, IFaction faction2)
        {
            if (LastWarTime.TryGetValue(CreateKey(faction1, faction2), out var value))
            {
                return value;
            }
            else
            {
                return default;
            }
        }

        public static bool HasDeclareWarCooldown(IFaction faction1, IFaction faction2, out float elapsedTime)
        {
            var campaignTime = GetLastWarTimeBetweenFactions(faction1, faction2);
            if (campaignTime.HasValue)
            {
                elapsedTime = campaignTime.Value.ElapsedDaysUntilNow;
                return campaignTime.Value.ElapsedDaysUntilNow < Settings.Instance.DeclareWarCooldownInDays;
            }
            else
            {
                elapsedTime = default;
                return false;
            }
        }

        public static bool HasPeaceProposalCooldownWithPlayerKingdom(Kingdom kingdom)
        {
            return GetLastPeaceProposalTime(kingdom).HasValue && GetLastPeaceProposalTime(kingdom).Value.ElapsedDaysUntilNow < MinimumDaysBetweenPeaceProposals;
        }

        public static bool HasPeaceProposalCooldown(Kingdom kingdomProposingPeace, Kingdom otherKingdom)
        {
            return (!HasExceededMinimumWarDuration(kingdomProposingPeace, otherKingdom, out var elapsedTime)
                || (otherKingdom.Leader.IsHumanPlayerCharacter && HasPeaceProposalCooldownWithPlayerKingdom(kingdomProposingPeace)));
        }

        public static bool HasExceededMinimumWarDuration(IFaction faction1, IFaction faction2, out float elapsedTime)
        {
            elapsedTime = -1f;
            var stanceLink = faction1.GetStanceWith(faction2);
            if (stanceLink?.IsAtWar ?? false)
            {
                elapsedTime = stanceLink.WarStartDate.ElapsedDaysUntilNow;
                var minimumWarDurationInDays = Settings.Instance.MinimumWarDurationInDays;
                return elapsedTime >= minimumWarDurationInDays;
            }
            return true;
        }

        public static CampaignTime? GetLastWarTimeWithPlayerFaction(IFaction faction)
        {
            return GetLastWarTimeBetweenFactions(faction, Hero.MainHero.MapFaction);
        }

        public static CampaignTime? GetLastPeaceProposalTime(Kingdom kingdom)
        {
            if (LastPeaceProposalTime.TryGetValue(kingdom, out var value))
            {
                return value;
            }
            return default;
        }

        internal void Sync()
        {
            if (_lastWarTime is null)
            {
                _lastWarTime = new Dictionary<string, CampaignTime>();
            }
            if (_lastPeaceProposalTime is null)
            {
                _lastPeaceProposalTime = new Dictionary<Kingdom, CampaignTime>();

            }
            if (_lastAllianceFormedTime is null)
            {
                _lastAllianceFormedTime = new Dictionary<string, CampaignTime>();
            }

            LastWarTime = _lastWarTime;
            LastPeaceProposalTime = _lastPeaceProposalTime;
            LastAllianceFormedTime = _lastAllianceFormedTime;

            foreach (var key in _lastWarTime.Keys)
            {
                var duration = Settings.Instance.DeclareWarCooldownInDays - _lastWarTime[key].ElapsedDaysUntilNow;
                if (duration > 0 & DecodeKeyToKingdoms(key, out var kingdoms))
                {
                    FormNonAggressionPactAction.Apply(kingdoms.Item1, kingdoms.Item2, bypassCosts: true, customDurationInDays: duration, queryPlayer: false);
                }
            }
            _lastWarTime.Clear();
        }
    }
}
