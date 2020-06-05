using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes
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

        private static float MinimumDaysBetweenPeaceProposals { get { return 5f; } }

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

        public void UpdateLastWarTime(IFaction faction1, IFaction faction2, CampaignTime campaignTime)
        {
            string key = CreateKey(faction1, faction2);
            _lastWarTime[key] = campaignTime;
        }

        public void UpdateLastAllianceFormedTime(Kingdom kingdom1, Kingdom kingdom2, CampaignTime campaignTime)
        {
            string key = CreateKey(kingdom1, kingdom2);
            _lastAllianceFormedTime[key] = campaignTime;
        }

        public static bool HasBreakAllianceCooldown(Kingdom kingdom1, Kingdom kingdom2, out float elapsedDaysUntilNow)
        {
            bool hasBreakAllianceCooldown = false;
            if (LastAllianceFormedTime.TryGetValue(CreateKey(kingdom1, kingdom2), out CampaignTime value))
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
            List<IFaction> factions = new List<IFaction> { faction1, faction2 };
            IEnumerable<string> keyArguments = factions.OrderBy(faction => faction.StringId).Select(faction => faction.StringId);
            return string.Join("+", keyArguments);
        }

        public static CampaignTime? GetLastWarTimeBetweenFactions(IFaction faction1, IFaction faction2)
        {
            if (LastWarTime.TryGetValue(CreateKey(faction1, faction2), out CampaignTime value))
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
            CampaignTime? campaignTime = GetLastWarTimeBetweenFactions(faction1, faction2);
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
            return (!HasExceededMinimumWarDuration(kingdomProposingPeace, otherKingdom, out float elapsedTime)
                || (otherKingdom.Leader.IsHumanPlayerCharacter && HasPeaceProposalCooldownWithPlayerKingdom(kingdomProposingPeace)));
        }

        public static bool HasExceededMinimumWarDuration(IFaction faction1, IFaction faction2, out float elapsedTime)
        {
            elapsedTime = -1f;
            StanceLink stanceLink = faction1.GetStanceWith(faction2);
            if (stanceLink?.IsAtWar ?? false)
            {
                elapsedTime = stanceLink.WarStartDate.ElapsedDaysUntilNow;
                int minimumWarDurationInDays = Settings.Instance.MinimumWarDurationInDays;
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
            if (LastPeaceProposalTime.TryGetValue(kingdom, out CampaignTime value))
            {
                return value;
            }
            return default;
        }

        internal void Sync()
        {
            if (_lastWarTime == null)
            {
                _lastWarTime = new Dictionary<string, CampaignTime>();
            }
            if (_lastPeaceProposalTime == null)
            {
                _lastPeaceProposalTime = new Dictionary<Kingdom, CampaignTime>();

            }
            if (_lastAllianceFormedTime == null)
            {
                _lastAllianceFormedTime = new Dictionary<string, CampaignTime>();
            }

            LastWarTime = _lastWarTime;
            LastPeaceProposalTime = _lastPeaceProposalTime;
            LastAllianceFormedTime = _lastAllianceFormedTime;
        }
    }
}
