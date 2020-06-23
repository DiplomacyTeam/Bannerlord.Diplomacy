using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace DiplomacyFixes.Costs
{
    class GoldCost : DiplomacyCost
    {
        private readonly Hero _giver;
        private readonly Hero _receiver;

        public GoldCost(Hero giver, Hero receiver, float value) : base(value) 
        {
            this._giver = giver;
            this._receiver = receiver;
        }

        public override void ApplyCost()
        {
            if (_giver != null && _receiver != null)
            {
                GiveGoldAction.ApplyBetweenCharacters(_giver, _receiver, (int)Value);

            }
            else if (_giver != null)
            {
                _giver.ChangeHeroGold(-(int)Value);
            }
            else if (_receiver != null)
            {
                _receiver.ChangeHeroGold((int)Value);
            }
        }

        public override bool CanPayCost()
        {
            if (_giver == null)
            {
                return true;
            }
            return _giver.Gold >= (int)Value;
        }
    }
}
