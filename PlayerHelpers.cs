using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes
{
    class PlayerHelpers
    {
        public static bool IsPlayerLeaderOfFaction(IFaction faction)
        {
            return faction == Hero.MainHero.MapFaction && faction.Leader == Hero.MainHero;
        }
    }
}
