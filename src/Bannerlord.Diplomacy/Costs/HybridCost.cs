using System.Collections.Generic;
using System.Linq;

namespace Diplomacy.Costs
{
    class HybridCost : DiplomacyCost
    {
        public InfluenceCost InfluenceCost { get; init; }
        public GoldCost GoldCost { get; init; }
        public List<KingdomWalletCost> KingdomWalletCosts { get; init; }

        public HybridCost(InfluenceCost influenceCost, GoldCost goldCost) : this(influenceCost, goldCost, new List<KingdomWalletCost>()) { }
        public HybridCost(InfluenceCost influenceCost, GoldCost goldCost, KingdomWalletCost kingdomWalletCost) : this(influenceCost, goldCost, new List<KingdomWalletCost>() { kingdomWalletCost }) { }
        public HybridCost(InfluenceCost influenceCost, GoldCost goldCost, List<KingdomWalletCost> kingdomWalletCosts) : base(new(kingdomWalletCosts) { influenceCost, goldCost })
        {
            InfluenceCost = influenceCost;
            GoldCost = goldCost;
            KingdomWalletCosts = kingdomWalletCosts;
        }

        public override bool CanPayCost()
        {
            return _diplomacyCosts.All(x => x.CanPayCost());
        }
    }
}