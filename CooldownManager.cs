using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes
{
    [SaveableClass(3)]
    class CooldownManager
    {
        internal static Dictionary<string, CampaignTime> LastWarTime { get; private set; } = new Dictionary<string, CampaignTime>();
        internal static Dictionary<Kingdom, CampaignTime> LastPeaceProposalTime { get; private set; } = new Dictionary<Kingdom, CampaignTime>();

        [SaveableField(1)]
        private Dictionary<string, CampaignTime> _lastWarTime;

        [SaveableField(2)]
        private Dictionary<Kingdom, CampaignTime> _lastPeaceProposalTime;

        private static float MinimumDaysBetweenPeaceProposals { get { return 5f; } }

        internal CooldownManager()
        {
            _lastWarTime = new Dictionary<string, CampaignTime>();
            _lastPeaceProposalTime = new Dictionary<Kingdom, CampaignTime>();
            LastWarTime = _lastWarTime;
            LastPeaceProposalTime = _lastPeaceProposalTime;
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

        public static bool HasDeclareWarCooldown(IFaction faction1, IFaction faction2)
        {
            CampaignTime? campaignTime = GetLastWarTimeBetweenFactions(faction1, faction2);
            if (campaignTime.HasValue)
            {
                return campaignTime.Value.ElapsedDaysUntilNow < Settings.Instance.DeclareWarCooldownInDays;
            }
            else
            {
                return false;
            }
        }

        public static bool HasPeaceProposalCooldown(Kingdom kingdom)
        {
            return GetLastPeaceProposalTime(kingdom).HasValue && GetLastPeaceProposalTime(kingdom).Value.ElapsedDaysUntilNow < MinimumDaysBetweenPeaceProposals;
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
            LastWarTime = _lastWarTime;
            LastPeaceProposalTime = _lastPeaceProposalTime;
        }
    }
}
