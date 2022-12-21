using Diplomacy.CivilWar.Factions;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Diplomacy.CivilWar.Actions
{
    public class ConsolidateKingdomsAction
    {
        private static void Apply(Kingdom rebelKingdom, Kingdom parentKingdom)
        {
            var rebelKingdomClans = new List<Clan>(rebelKingdom.Clans);

            foreach (var clan in rebelKingdomClans)
            {
                // make sure to retain influence
                var influence = clan.Influence;
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
            foreach (var fief in rebelFaction.OriginalFiefOwners.Keys)
            {
                var currentOwner = fief.OwnerClan;
                if (currentOwner.Kingdom != rebelFaction.ParentKingdom)
                    continue;

                var originalOwner = rebelFaction.OriginalFiefOwners[fief];
                if (currentOwner != originalOwner && originalOwner.Kingdom == rebelFaction.ParentKingdom)
                {
                    ChangeOwnerOfSettlementAction.ApplyByDefault(originalOwner.Leader, fief.Settlement);
                }
            }
        }
    }
}