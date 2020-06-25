using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace DiplomacyFixes
{
    class DiplomacyCostManager
    {
        public static void PayWarReparations(Kingdom kingdom, Kingdom otherKingdom, int goldCost)
        {
            if (goldCost >= kingdom.Leader.Gold)
            {
                GiveGoldAction.ApplyBetweenCharacters(kingdom.Leader, otherKingdom.Leader, kingdom.Leader.Gold);
            }
            else
            {
                GiveGoldAction.ApplyBetweenCharacters(kingdom.Leader, otherKingdom.Leader, goldCost);
            }
        }
    }
}
