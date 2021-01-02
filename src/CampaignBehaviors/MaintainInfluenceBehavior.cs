using Diplomacy.Extensions;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Barterables;

namespace Diplomacy.CampaignBehaviors
{
    class MaintainInfluenceBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, ReduceCorruption);
        }

        public override void SyncData(IDataStore dataStore) { }

        private void ReduceCorruption(Clan clan)
        {
            if (clan.MapFaction.IsKingdomFaction && !clan.Leader.IsHumanPlayerCharacter && clan.InfluenceChange < 0 && clan.GetCorruption() > 0)
            {
                var fiefBarterable = GetBestFiefBarter(clan, out var targetClan);
                if (fiefBarterable is not null)
                {
                    var goldValue = GetGoldValueForFief(targetClan, fiefBarterable.TargetSettlement);
                    var goldBarterable = new GoldBarterable(targetClan.Leader, clan.Leader, null, null, goldValue);
                    goldBarterable.CurrentAmount = goldValue;

                    fiefBarterable.Apply();
                    goldBarterable.Apply();
                }
            }
        }

        private FiefBarterable? GetBestFiefBarter(Clan ownerClan, out Clan? otherClan)
        {
            var settlementToTrade = ownerClan.GetPermanentFiefs()
                .OrderBy(settlement => settlement.Prosperity)
                .FirstOrDefault()?.Settlement;

            var targetClan = ((Kingdom)ownerClan.MapFaction).Clans
                .Where(clan => clan != ownerClan && !clan.HasMaximumFiefs() && !clan.IsUnderMercenaryService && clan != Clan.PlayerClan)?
                .OrderByDescending(clan => GetGoldValueForFief(clan, settlementToTrade))?
                .FirstOrDefault();

            if (settlementToTrade is not null && targetClan is not null)
            {
                var fiefBarterable = new FiefBarterable(settlementToTrade, ownerClan.Leader, targetClan.Leader);
                otherClan = targetClan;
                return fiefBarterable;
            }
            else
            {
                otherClan = default;
                return default;
            }
        }

        private static int GetGoldValueForFief(Clan targetClan, Settlement settlement)
        {
            return (int)Math.Min(Campaign.Current.Models.SettlementValueModel.CalculateValueForFaction(settlement, targetClan),
                                 targetClan.Gold * 0.8);
        }
    }
}
