using System.Linq;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.Extensions
{
    // thanks to TH3UNKN0WN for this code
    public static class BaseKingdomInfoExtension
    {
        public static bool IsInsideTeritoryOf(this Kingdom kingdom, Kingdom kingdomToCheck)
        {
            var kingdomPositions = kingdom.Settlements.Where(go => go.IsTown || go.IsCastle).Select(go => go.Position2D);
            var kingdomToCheckPositions = kingdomToCheck.Settlements.Where(go => go.IsTown || go.IsCastle).Select(go => go.Position2D);

            if (!(kingdomPositions?.Any() ?? false) || !(kingdomToCheckPositions?.Any() ?? false))
            {
                return false;
            }

            return kingdomPositions.Max(p => p.X) <= kingdomToCheckPositions.Max(p => p.X) &&
                kingdomPositions.Max(p => p.Y) <= kingdomToCheckPositions.Max(p => p.Y) &&
                kingdomPositions.Min(p => p.X) >= kingdomToCheckPositions.Min(p => p.X) &&
                kingdomPositions.Min(p => p.Y) >= kingdomToCheckPositions.Min(p => p.Y);
        }
    }
}
