using System.Collections.Generic;
using System.Linq;

namespace Diplomacy.Costs
{
    class HybridCost : DiplomacyCost
    {
        readonly List<DiplomacyCost> _diplomacyCosts;

        public InfluenceCost InfluenceCost { get; }
        public GoldCost GoldCost { get; }

        public HybridCost(InfluenceCost influenceCost, GoldCost goldCost) : base(0f)
        {
            InfluenceCost = influenceCost;
            GoldCost = goldCost;
            _diplomacyCosts = new List<DiplomacyCost> { InfluenceCost, GoldCost };
        }

        public override void ApplyCost()
        {
            _diplomacyCosts.ForEach(x => x.ApplyCost());
        }

        public override bool CanPayCost()
        {
            return _diplomacyCosts.All(x => x.CanPayCost());
        }
    }
}