using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace DiplomacyFixes.ViewModel
{
    public class KingdomTruceItemVMExtensionVM : KingdomTruceItemVM
    {
        // private static string INFLUENCE_COST = "Influence Cost: {0}";

        public KingdomTruceItemVMExtensionVM(IFaction faction1, IFaction faction2, Action<KingdomDiplomacyItemVM> onSelection, Action<KingdomTruceItemVM> onAction) : base(faction1, faction2, onSelection, onAction)
        {
            _isOptionAvailable = true;
            _influenceCost = 0;
            _isMessengerAvailable = true;
            _sendMessengerInfluenceCost = 0;
            UpdateDiplomacyProperties();
        }

        protected override void UpdateDiplomacyProperties()
        {
            base.UpdateDiplomacyProperties();
            this._isOptionAvailable = WarAndPeaceConditions.canDeclareWarExceptions(this).IsEmpty();
            this._influenceCost = (int)CostCalculator.DetermineInfluenceCostForDeclaringWar();
            float sendMessengerInfluenceCost = CostCalculator.DetermineInfluenceCostForSendingMessenger();
            this._isMessengerAvailable = MessengerUtil.CanSendMessengerWithInfluenceCost(Faction2Leader.Hero, sendMessengerInfluenceCost);
            this._sendMessengerInfluenceCost = (int)sendMessengerInfluenceCost;
        }

        protected void SendMessenger()
        {
            MessengerUtil.SendMessengerWithInfluenceCost(Faction2Leader.Hero, CostCalculator.DetermineInfluenceCostForSendingMessenger());
        }

        [DataSourceProperty]
        public int SendMessengerInfluenceCost
        {
            get
            {
                return this._sendMessengerInfluenceCost;
            }
            set
            {
                if (value != this._sendMessengerInfluenceCost)
                {
                    this._sendMessengerInfluenceCost = value;
                    base.OnPropertyChanged("SendMessengerInfluenceCost");
                }
            }
        }

        [DataSourceProperty]
        public bool IsMessengerAvailable
        {
            get
            {
                return this._isMessengerAvailable;
            }
            set
            {
                if (value != this._isMessengerAvailable)
                {
                    this._isMessengerAvailable = value;
                    base.OnPropertyChanged("IsMessengerAvailable");
                }
            }
        }

        [DataSourceProperty]
        public int InfluenceCost
        {
            get
            {
                return this._influenceCost;
            }
            set
            {
                if (value != this._influenceCost)
                {
                    this._influenceCost = value;
                    base.OnPropertyChanged("InfluenceCost");
                }
            }
        }

        [DataSourceProperty]
        public bool IsOptionAvailable
        {
            get
            {
                return this._isOptionAvailable;
            }
            set
            {
                if (value != this._isOptionAvailable)
                {
                    this._isOptionAvailable = value;
                    base.OnPropertyChanged("IsOptionAvailable");
                }
            }
        }

        private bool _isOptionAvailable;
        private int _influenceCost;
        private bool _isMessengerAvailable;
        private int _sendMessengerInfluenceCost;
    }
}
