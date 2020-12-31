using System.Collections.Generic;
using System.Linq;

namespace Diplomacy.Costs
{
    class HybridCost : DiplomacyCost
    {
        List<DiplomacyCost> _diplomacyCosts;

        public InfluenceCost InfluenceCost { get; }
        public GoldCost GoldCost { get; }

        public HybridCost(InfluenceCost influenceCost, GoldCost goldCost) : base(0f)
        {
            this.InfluenceCost = influenceCost;
            this.GoldCost = goldCost;
            this._diplomacyCosts = new List<DiplomacyCost> { InfluenceCost, GoldCost };
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
