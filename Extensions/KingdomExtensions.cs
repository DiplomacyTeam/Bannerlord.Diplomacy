
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.Extensions
{
    public static class KingdomExtensions
    {
        public static float GetExpansionism(this Kingdom kingdom)
        {
            return ExpansionismManager.Instance.GetExpansionism(kingdom);
        }
    }
}
