using JetBrains.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Diplomacy.Costs
{
    public class InfluenceCost : DiplomacyCost
    {
        private readonly Clan? _clan;

        public static readonly InfluenceCost NullCost = new();

        public InfluenceCost([NotNull]Clan clan, float value) : base(value)
        {
            _clan = clan;
        }

        private InfluenceCost() : base(0f)
        {
            _clan = null;
        }

        public override void ApplyCost()
        {
            if (_clan is null)
            {
                return;
            }
            _clan.Influence = MBMath.ClampFloat(_clan.Influence - Value, 0f, float.MaxValue);
        }

        public override bool CanPayCost()
        {
            if (_clan is null)
            {
                return true;
            }
            return _clan.Influence >= Value;
        }
    }
}
