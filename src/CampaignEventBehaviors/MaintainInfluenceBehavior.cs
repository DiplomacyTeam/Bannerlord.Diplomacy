using Diplomacy.Extensions;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Barterables;

namespace Diplomacy.CampaignEventBehaviors
{
    class MaintainInfluenceBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, ReduceCorruption);
        }

        private void ReduceCorruption(Clan clan)
        {
            if (clan.MapFaction.IsKingdomFaction && !clan.Leader.IsHumanPlayerCharacter && clan.InfluenceChange < 0 && clan.GetCorruption() > 0)
            {

                FiefBarterable fiefBarterable = GetBestFiefBarter(clan, out Clan targetClan);
                if (fiefBarterable != null)
                {
                    int goldValue = GetGoldValueForFief(targetClan, fiefBarterable.TargetSettlement);
                    GoldBarterable goldBarterable = new GoldBarterable(targetClan.Leader, clan.Leader, null, null, goldValue);
                    goldBarterable.CurrentAmount = goldValue;

                    fiefBarterable.Apply();
                    goldBarterable.Apply();
                }
            }
        }

        private FiefBarterable GetBestFiefBarter(Clan ownerClan, out Clan otherClan)
        {
            Settlement settlementToTrade = ownerClan.GetPermanentFiefs().OrderBy(settlement => settlement.Prosperity).FirstOrDefault()?.Settlement;
            Clan targetClan = (ownerClan.MapFaction as Kingdom).Clans.Where(clan => clan != ownerClan && !clan.HasMaximumFiefs() && !clan.IsUnderMercenaryService && clan != Clan.PlayerClan)?.OrderByDescending(clan => GetGoldValueForFief(clan, settlementToTrade))?.FirstOrDefault();
            if (settlementToTrade != null && targetClan != null)
            {
                FiefBarterable fiefBarterable = new FiefBarterable(settlementToTrade, ownerClan.Leader, targetClan.Leader);
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
            return (int)Math.Min(Campaign.Current.Models.SettlementValueModel.CalculateValueForFaction(settlement, targetClan), targetClan.Gold * 0.8);
        }

        public override void SyncData(IDataStore dataStore) { }
    }
}
