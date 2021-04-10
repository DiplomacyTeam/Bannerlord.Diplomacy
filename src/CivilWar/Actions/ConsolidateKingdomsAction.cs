using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Diplomacy.CivilWar
{
    public class ConsolidateKingdomsAction
    {
        public static void Apply(Kingdom rebelKingdom, Kingdom parentKingdom)
        {
            var rebelKingdomClans = new List<Clan>(rebelKingdom.Clans);

            foreach (Clan clan in rebelKingdomClans)
            {
                ChangeKingdomAction.ApplyByJoinToKingdom(clan, parentKingdom, false);
            }

            DestroyKingdomAction.Apply(rebelKingdom);
        }
    }
}
