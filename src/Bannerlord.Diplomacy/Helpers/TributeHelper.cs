using Diplomacy.Extensions;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.Election;

using static Diplomacy.WarExhaustion.WarExhaustionManager;

namespace Diplomacy.Helpers
{
    internal static class TributeHelper
    {
        internal static int GetDailyTribute(Kingdom kingdomInQuestion, Kingdom otherKingdom)
        {
            var atWar = FactionManager.IsAtWarAgainstFaction(kingdomInQuestion, otherKingdom);
            if (!atWar)
            {
                var stance = kingdomInQuestion.GetStanceWith(otherKingdom);
                return stance?.GetDailyTributePaid(kingdomInQuestion) ?? 0;
            }

            if (kingdomInQuestion == Clan.PlayerClan.Kingdom)
            {
                var currentItemsUnresolvedDecision = kingdomInQuestion.UnresolvedDecisions.OfType<MakePeaceKingdomDecision>().FirstOrDefault(d => d.FactionToMakePeaceWith == otherKingdom && !d.ShouldBeCancelled());
                if (currentItemsUnresolvedDecision != null)
                {
                    return currentItemsUnresolvedDecision.DailyTributeToBePaid;
                }
            }

            if (kingdomInQuestion.IsRebelKingdomOf(otherKingdom) || otherKingdom.IsRebelKingdomOf(kingdomInQuestion))
                return 0;

            int valueForOtherKingdom = GetBaseValueForTrubute(kingdomInQuestion, otherKingdom);

            if (Settings.Instance!.EnableWarExhaustion)
            {
                var warResult = Instance!.GetWarResult(kingdomInQuestion, otherKingdom);
                var warResultForOtherKingdom = Instance.GetWarResult(otherKingdom, kingdomInQuestion);
                //Neither kingdom should get any tribute when there's a tie or PyrrhicVictory
                if (Min(warResult, warResultForOtherKingdom) != WarResult.None && Max(warResult, warResultForOtherKingdom) <= WarResult.PyrrhicVictory)
                {
                    valueForOtherKingdom = 0;
                }
                //Loosing kingdom should not receive tribute from the winner
                if (valueForOtherKingdom < 0 && warResult == WarResult.Loss && warResultForOtherKingdom > WarResult.PyrrhicVictory)
                {
                    valueForOtherKingdom = 0;
                }
                //...and winning kingdom should not pay a tribute to the loser
                if (valueForOtherKingdom > 0 && warResult > WarResult.PyrrhicVictory && warResultForOtherKingdom == WarResult.Loss)
                {
                    valueForOtherKingdom = 0;
                }
            }
            var dailyPeaceTributeToPay = Campaign.Current.Models.DiplomacyModel.GetDailyTributeForValue(valueForOtherKingdom);
            return 10 * (dailyPeaceTributeToPay / 10);
        }

        internal static int GetBaseValueForTrubute(Kingdom kingdomInQuestion, Kingdom otherKingdom)
        {
            var peaceBarterable = new PeaceBarterable(kingdomInQuestion.Leader, kingdomInQuestion, otherKingdom, CampaignTime.Years(1f));
            var valueForOtherKingdom = -peaceBarterable.GetValueForFaction(otherKingdom);
            foreach (Clan clan in otherKingdom.Clans)
            {
                var valueForClan = -peaceBarterable.GetValueForFaction(clan);
                if (valueForClan > valueForOtherKingdom)
                    valueForOtherKingdom = valueForClan;
            }
            if (valueForOtherKingdom > -5000 && valueForOtherKingdom < 5000)
                valueForOtherKingdom = 0;
            return valueForOtherKingdom;
        }
    }
}