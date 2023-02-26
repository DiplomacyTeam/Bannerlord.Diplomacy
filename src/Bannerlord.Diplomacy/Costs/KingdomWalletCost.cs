using TaleWorlds.CampaignSystem;

using static Diplomacy.Actions.GiveGoldToKingdomAction;

namespace Diplomacy.Costs
{
    public sealed class KingdomWalletCost : AbstractDiplomacyCost
    {
        private readonly Kingdom? _payer;
        private readonly WalletType _payerWallet;

        private readonly Kingdom? _receiver;
        private readonly WalletType _receiverWallet;

        public Kingdom? PayingKingdom => _payer;
        public WalletType PayerWallet => _payerWallet;
        public Kingdom? ReceivingKingdom => _receiver;
        public WalletType ReceiverWallet => _receiverWallet;

        public KingdomWalletCost(Kingdom? payer, Kingdom? receiver, float value, WalletType payerWallet = WalletType.ReparationsWallet, WalletType receiverWallet = WalletType.ReparationsWallet) : base(value)
        {
            _payer = payer;
            _payerWallet = payerWallet;
            _receiver = receiver;
            _receiverWallet = receiverWallet;
        }

        public override void ApplyCost() => ApplyFromWalletToWallet(_payer, _receiver, (int) Value, _payerWallet, _receiverWallet);
        public override bool CanPayCost() => true;
    }
}