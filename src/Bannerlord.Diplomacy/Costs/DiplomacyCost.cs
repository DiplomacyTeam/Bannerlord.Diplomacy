using System.Collections.Generic;
using System.Linq;

namespace Diplomacy.Costs
{
    public class DiplomacyCost : IDiplomacyCost
    {
        protected readonly List<IDiplomacyCost> _diplomacyCosts;

        public List<IDiplomacyCost> DiplomacyCosts => _diplomacyCosts;

        public DiplomacyCost(List<IDiplomacyCost> diplomacyCosts)
        {
            _diplomacyCosts = diplomacyCosts;
        }

        public void ApplyCost()
        {
            _diplomacyCosts.ForEach(x => x.ApplyCost());
        }

        public virtual bool CanPayCost()
        {
            var listedDiplomacyCosts = _diplomacyCosts.OfType<DiplomacyCost>().ToList();
            var unlistedCosts = _diplomacyCosts.Except(listedDiplomacyCosts).Union(listedDiplomacyCosts.SelectMany(c => c._diplomacyCosts));

            //is there more generic way to handle those?
            var grouppedInfluenceCosts = unlistedCosts.OfType<InfluenceCost>().GroupBy(c => c.Clan).Select(gr => new InfluenceCost(gr.Key, gr.Sum(x => x.Value))).ToList();
            var grouppedgoldCosts = unlistedCosts.OfType<GoldCost>().GroupBy(c => c.Giver).Select(gr => new GoldCost(gr.Key, gr.Sum(x => x.Value))).ToList();

            return grouppedInfluenceCosts.All(x => x.CanPayCost()) && grouppedgoldCosts.All(x => x.CanPayCost());
        }
    }
}