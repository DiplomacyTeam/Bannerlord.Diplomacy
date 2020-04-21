using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;

namespace DiplomacyFixes
{
    class CooldownManager
    {
        private static Dictionary<IFaction, CampaignTime> _lastWarTime;

        public static void UpdateLastWarTime(IFaction faction, CampaignTime campaignTime)
        {
            if (_lastWarTime == null)
            {
                _lastWarTime = new Dictionary<IFaction, CampaignTime>();
            }

            _lastWarTime[faction] = campaignTime;
        }

        public static Nullable<CampaignTime> GetLastWarTimeWithFaction(IFaction faction)
        {
            if (_lastWarTime.TryGetValue(faction, out CampaignTime value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }
    }
}
