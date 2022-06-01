using Diplomacy.Extensions;

using System;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.Settlements;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class MaintainInfluenceBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents() => CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, ReduceCorruption);

        public override void SyncData(IDataStore dataStore) { }

        private void ReduceCorruption(Clan clan)
        {
            if (!clan.MapFaction.IsKingdomFaction || clan.GetCorruption() <= 0 || clan.Leader.IsHumanPlayerCharacter)
                return;

            var influenceChange = Campaign.Current.Models.ClanPoliticsModel.CalculateInfluenceChange(clan).ResultNumber;

            if (influenceChange > 0)
                return;

            var fiefBarterable = GetBestFiefBarter(clan, out var targetClan);

            if (fiefBarterable is null || targetClan is null)
                return;

            var goldValue = GetGoldValueForFief(targetClan, fiefBarterable.TargetSettlement);
            var goldBarterable = new GoldBarterable(targetClan.Leader, clan.Leader, null, null, goldValue) { CurrentAmount = goldValue };

            fiefBarterable.Apply();
            goldBarterable.Apply();
        }

        private FiefBarterable? GetBestFiefBarter(Clan ownerClan, out Clan? otherClan)
        {
            otherClan = default;

            var settlementToTrade = ownerClan.GetPermanentFiefs()
                .OrderBy(settlement => settlement.Prosperity)
                .FirstOrDefault()?.Settlement;

            if (settlementToTrade is null)
                return default;

            var targetClan = ((Kingdom) ownerClan.MapFaction).Clans
                .Where(c => c != ownerClan && !c.HasMaximumFiefs() && !c.IsUnderMercenaryService && c != Clan.PlayerClan)
                .OrderByDescending(c => GetGoldValueForFief(c, settlementToTrade))
                .FirstOrDefault();

            if (targetClan is null)
                return default;

            var fiefBarterable = new FiefBarterable(settlementToTrade, ownerClan.Leader, targetClan.Leader);
            otherClan = targetClan;
            return fiefBarterable;
        }

        private static int GetGoldValueForFief(Clan buyerClan, Settlement settlement)
        {
            return (int) Math.Min(Campaign.Current.Models.SettlementValueModel.CalculateSettlementValueForFaction(settlement, buyerClan),
                                 buyerClan.Gold * 0.8);
        }
    }
}