using Diplomacy.CivilWar.Factions;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Diplomacy.CivilWar.Actions
{
    public class ConsolidateKingdomsAction
    {
        internal static void Apply(Kingdom rebelKingdom, Kingdom parentKingdom)
        {
            // The game destroys a kingdom left without settlements as soon as a clan leaves it, and every clan
            // still in it is pushed out with no influence. Moving the clans that hold fiefs last keeps the
            // kingdom's settlements until no other clan is left to lose its influence.
            var rebelKingdomClans = rebelKingdom.Clans.Where(c => !c.IsEliminated).OrderBy(c => c.Settlements.Count > 0).ToList();

            foreach (var clan in rebelKingdomClans)
            {
                // make sure to retain influence
                var influence = clan.Influence;
                ChangeKingdomAction.ApplyByJoinToKingdom(clan, parentKingdom, showNotification: false);
                clan.Influence = influence;
            }

            // The last clan leaving may already have made the game destroy it.
            if (!rebelKingdom.IsEliminated)
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
                if (!originalOwner.IsEliminated && currentOwner != originalOwner && originalOwner.Kingdom == rebelFaction.ParentKingdom)
                {
                    ChangeOwnerOfSettlementAction.ApplyByDefault(originalOwner.Leader, fief.Settlement);
                }
            }
        }
    }
}