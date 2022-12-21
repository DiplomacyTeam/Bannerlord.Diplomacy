using System.Linq;

namespace Diplomacy.Costs
{
    class HybridCost : DiplomacyCost
    {
        public InfluenceCost InfluenceCost { get; }
        public GoldCost GoldCost { get; }

        public HybridCost(InfluenceCost influenceCost, GoldCost goldCost): base(new() { influenceCost, goldCost })
        {
            InfluenceCost = influenceCost;
            GoldCost = goldCost;
        }

        public override bool CanPayCost()
        {
            return _diplomacyCosts.All(x => x.CanPayCost());
        }
    }
}