using TaleWorlds.CampaignSystem;

namespace Diplomacy
{
    internal sealed class PlayerHelpers
    {
        public static Kingdom? GetOpposingKingdomIfPlayerKingdomProvided(Kingdom kingdom1, Kingdom kingdom2)
        {
            return ((kingdom1 == Hero.MainHero.MapFaction) ^ (kingdom2 == Hero.MainHero.MapFaction))
                ? kingdom1 == Hero.MainHero.MapFaction ? kingdom2 : kingdom1
                : default;
        }
    }
}
