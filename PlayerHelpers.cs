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
