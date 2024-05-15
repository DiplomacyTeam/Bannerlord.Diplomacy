using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Diplomacy.Actions
{
    public static class SoftlyDestroyKingdomAction
    {
        public static void Apply(Kingdom kingdomToDestroy)
        {
            //Cold be already eliminated via CW or other mods
            if (kingdomToDestroy.IsEliminated)
                return;

            foreach (Clan clan in kingdomToDestroy.Clans.ToList())
            {
                if (!clan.IsEliminated)
                {
                    if (clan.IsUnderMercenaryService)
                    {
                        ChangeKingdomAction.ApplyByLeaveKingdomAsMercenary(clan);
                    }
                    else
                    {
                        ChangeKingdomAction.ApplyByLeaveKingdom(clan);
                    }
                }
            }
            //Check again!
            if (!kingdomToDestroy.IsEliminated) DestroyKingdomAction.Apply(kingdomToDestroy);
        }
    }
}