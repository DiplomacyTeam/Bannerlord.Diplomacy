using Diplomacy.Actions;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace Diplomacy.Costs
{
    public sealed class GoldCost : AbstractDiplomacyCost
    {
        private readonly Hero _giver;

        private readonly Hero? _receiver = null;
        private readonly Clan? _receivingClan = null;
        private readonly PartyBase? _receivingParty = null;
        private readonly Settlement? _receivingSettlement = null;

        public Hero Giver => _giver;

        public GoldCost(Hero giver, float value) : base(value)
        {
            _giver = giver;
        }

        public GoldCost(Hero giver, Hero? receiver, float value) : base(value)
        {
            _giver = giver;
            _receiver = receiver;
        }

        public GoldCost(Hero giver, Clan? receivingClan, float value) : base(value)
        {
            _giver = giver;
            _receivingClan = receivingClan;
        }

        public GoldCost(Hero giver, PartyBase? receivingParty, float value) : base(value)
        {
            _giver = giver;
            _receivingParty = receivingParty;
        }

        public GoldCost(Hero giver, Settlement? receivingSettlement, float value) : base(value)
        {
            _giver = giver;
            _receivingSettlement = receivingSettlement;
        }

        public override void ApplyCost()
        {
            if (_receiver is not null)
            {
                GiveGoldAction.ApplyBetweenCharacters(_giver, _receiver, (int) Value);
            }
            else if (_receivingClan is not null)
            {
                GiveGoldToClanAction.ApplyFromHeroToClan(_giver, _receivingClan, (int) Value);
            }
            else if (_receivingParty is not null)
            {
                GiveGoldAction.ApplyForCharacterToParty(_giver, _receivingParty, (int) Value);
            }
            else if (_receivingSettlement is not null)
            {
                GiveGoldAction.ApplyForCharacterToSettlement(_giver, _receivingSettlement, (int) Value);
            }
            else
            {
                _giver.ChangeHeroGold(-(int) Value);
            }
        }

        public override bool CanPayCost()
        {
            return _giver.Gold >= (int) Value;
        }
    }
}