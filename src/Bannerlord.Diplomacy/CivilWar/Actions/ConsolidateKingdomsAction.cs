using Diplomacy.CivilWar.Factions;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;

namespace Diplomacy.CivilWar.Actions
{
    public class ConsolidateKingdomsAction
    {
        private static void Apply(Kingdom rebelKingdom, Kingdom parentKingdom)
        {
            var rebelKingdomClans = new List<Clan>(rebelKingdom.Clans);

            foreach (Clan clan in rebelKingdomClans)
            {
                // make sure to retain influence
                float influence = clan.Influence;
                ChangeKingdomAction.ApplyByJoinToKingdom(clan, parentKingdom, false);
                clan.Influence = influence;
            }

            DestroyKingdomAction.Apply(rebelKingdom);
        }

        public static void Apply(RebelFaction rebelFaction)
        {
            if (!rebelFaction.AtWar)
            {
                return;
            }

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