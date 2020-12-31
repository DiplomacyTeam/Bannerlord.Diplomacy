using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Diplomacy.Costs
{
    class InfluenceCost : DiplomacyCost
    {
        private readonly Clan _clan;

        public InfluenceCost(Clan clan, float value) : base(value)
        {
            this._clan = clan;
        }

        public override void ApplyCost()
        {
            _clan.Influence = MBMath.ClampFloat(_clan.Influence - Value, 0f, float.MaxValue);
        }

        public override bool CanPayCost()
        {
            return _clan.Influence >= Value;
        }
    }
}
