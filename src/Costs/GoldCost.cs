using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Diplomacy.Costs
{
    class GoldCost : DiplomacyCost
    {
        private readonly Hero _giver;
        private readonly Hero _receiver;

        public GoldCost(Hero giver, Hero receiver, float value) : base(value)
        {
            _giver = giver;
            _receiver = receiver;
        }

        public override void ApplyCost()
        {
            if (_giver is not null && _receiver is not null)
            {
                GiveGoldAction.ApplyBetweenCharacters(_giver, _receiver, (int)Value);

            }
            else if (_giver is not null)
            {
                _giver.ChangeHeroGold(-(int)Value);
            }
            else if (_receiver is not null)
            {
                _receiver.ChangeHeroGold((int)Value);
            }
        }

        public override bool CanPayCost()
        {
            if (_giver is null)
            {
                return true;
            }
            return _giver.Gold >= (int)Value;
        }
    }
}
