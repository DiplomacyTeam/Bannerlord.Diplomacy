using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes
{
    [SaveableClass(3)]
    class CooldownManager
    {

        internal static Dictionary<string, CampaignTime> LastWarTime { get; private set; } = new Dictionary<string, CampaignTime>(); 

        [SaveableField(1)]
        private Dictionary<string, CampaignTime> _lastWarTime;

        internal CooldownManager()
        {
            _lastWarTime = new Dictionary<string, CampaignTime>();
            LastWarTime = _lastWarTime;
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

        public static bool HasActiveWarCooldown(IFaction faction1, IFaction faction2)
        {
            CampaignTime? campaignTime = GetLastWarTimeBetweenFactions(faction1, faction2);
            if(campaignTime.HasValue)
            {
                return campaignTime.Value.ElapsedDaysUntilNow < Settings.Instance.DeclareWarCooldownInDays;
            } else
            {
                return false;
            }
        }

        public static Nullable<CampaignTime> GetLastWarTimeWithPlayerFaction(IFaction faction)
        {
            return GetLastWarTimeBetweenFactions(faction, Hero.MainHero.MapFaction);
        }

        internal void sync()
        {
            LastWarTime = _lastWarTime;
        }
    }
}
