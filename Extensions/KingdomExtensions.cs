
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.Extensions
{
    public static class KingdomExtensions
    {
        public static float GetExpansionism(this Kingdom kingdom)
        {
            return ExpansionismManager.Instance.GetExpansionism(kingdom);
        }

        public static float GetMinimumExpansionism(this Kingdom kingdom)
        {
            return ExpansionismManager.Instance.GetMinimumExpansionism(kingdom); 
        }

        public static bool IsAlliedWith(this IFaction faction1, IFaction faction2)
        {
            if (faction1 == null || faction2 == null || faction1 == faction2)
            {
                return false;
            }
            StanceLink stanceLink = faction1.GetStanceWith(faction2);
            return stanceLink.IsAllied;
        }
    }
}
