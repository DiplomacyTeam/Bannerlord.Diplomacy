using Diplomacy.Actions;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.ObjectSystem;

namespace Diplomacy.Costs
{
    public sealed class GoldCost : AbstractDiplomacyCost
    {
        private readonly Hero _giver;

        private readonly MBObjectBase? _receiver = null;

        public Hero Giver => _giver;
        public bool HasReceiver => _receiver != null;
        public MBObjectBase? Receiver => _receiver;

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
            _receiver = receivingClan;
        }

        public GoldCost(Hero giver, Kingdom? receivingKingdom, float value) : base(value)
        {
            _giver = giver;
            _receiver = receivingKingdom;
        }

        public GoldCost(Hero giver, MobileParty? receivingParty, float value) : base(value)
        {
            _giver = giver;
            _receiver = receivingParty;
        }

        public GoldCost(Hero giver, Settlement? receivingSettlement, float value) : base(value)
        {
            _giver = giver;
            _receiver = receivingSettlement;
        }

        public override void ApplyCost()
        {
            if (Value <= 0f)
                return;

            switch (_receiver)
            {
                case Hero receiverHero:
                    GiveGoldAction.ApplyBetweenCharacters(_giver, receiverHero, (int) Value);
                    return;
                case Clan receivingClan:
                    GiveGoldToClanAction.ApplyFromHeroToClan(_giver, receivingClan, (int) Value);
                    return;
                case Kingdom receivingKingdom:
                    GiveGoldToKingdomAction.ApplyFromHeroToKingdom(_giver, receivingKingdom, (int) Value);
                    return;
                case MobileParty mobileParty:
                    GiveGoldAction.ApplyForCharacterToParty(_giver, mobileParty.Party, (int) Value);
                    return;
                case Settlement receivingSettlement:
                    GiveGoldAction.ApplyForCharacterToSettlement(_giver, receivingSettlement, (int) Value);
                    return;
                default:
                    _giver.ChangeHeroGold(-(int) Value);
                    return;
            }
        }

        public override bool CanPayCost() => _giver.Gold >= (int) Value;
    }
}