using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Diplomacy.CivilWar
{
    public class ConsolidateKingdomsAction
    {
        private static void Apply(Kingdom rebelKingdom, Kingdom parentKingdom)
        {
            var rebelKingdomClans = new List<Clan>(rebelKingdom.Clans);

            foreach (Clan clan in rebelKingdomClans)
            {
                ChangeKingdomAction.ApplyByJoinToKingdom(clan, parentKingdom, false);
            }

            DestroyKingdomAction.Apply(rebelKingdom);
        }

        public static void Apply(RebelFaction rebelFaction)
        {
            Apply(rebelFaction.RebelKingdom!, rebelFaction.ParentKingdom);

            // return fiefs to owners
            foreach (Town fief in rebelFaction.OriginalFiefOwners.Keys)
            {
                Clan currentOwner = fief.OwnerClan;
                if (currentOwner.Kingdom != rebelFaction.ParentKingdom)
                    continue;

                Clan originalOwner = rebelFaction.OriginalFiefOwners[fief];
                if (currentOwner != originalOwner && originalOwner.Kingdom == rebelFaction.ParentKingdom)
                {
                    ChangeOwnerOfSettlementAction.ApplyByDefault(originalOwner.Leader, fief.Settlement);
                }
            }

        }
    }
}
