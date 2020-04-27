using DiplomacyFixes.Messengers;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.ViewModel
{
    public class KingdomTruceItemVMExtensionVM : KingdomTruceItemVM
    {
        // private static string INFLUENCE_COST = "Influence Cost: {0}";

        public KingdomTruceItemVMExtensionVM(IFaction faction1, IFaction faction2, Action<KingdomDiplomacyItemVM> onSelection, Action<KingdomTruceItemVM> onAction) : base(faction1, faction2, onSelection, onAction)
        {
            this.IsOptionAvailable = true;
            this.InfluenceCost = 0;
            this.IsMessengerAvailable = true;
            this.SendMessengerInfluenceCost = 0;
            UpdateDiplomacyProperties();
        }

        protected override void UpdateDiplomacyProperties()
        {
            base.UpdateDiplomacyProperties();
            this.IsOptionAvailable = WarAndPeaceConditions.CanDeclareWarExceptions(this).IsEmpty();
            this.InfluenceCost = (int)DiplomacyCostCalculator.DetermineInfluenceCostForDeclaringWar();
            float sendMessengerInfluenceCost = DiplomacyCostCalculator.DetermineInfluenceCostForSendingMessenger();
            UpdateActionAvailability();
            this.SendMessengerInfluenceCost = (int)sendMessengerInfluenceCost;
            this.SendMessengerActionName = new TextObject("{=cXfcwzPp}Send Messenger").ToString();
        }

        private void UpdateActionAvailability()
        {
            float sendMessengerInfluenceCost = DiplomacyCostCalculator.DetermineInfluenceCostForSendingMessenger();
            this.IsMessengerAvailable = MessengerManager.CanSendMessengerWithInfluenceCost(Faction2Leader.Hero, sendMessengerInfluenceCost);
        }

        protected override void OnSelect()
        {
            base.OnSelect();
            UpdateActionAvailability();
        }

        protected void SendMessenger()
        {
            Events.Instance.OnMessengerSent(Faction2Leader.Hero);
            this.UpdateDiplomacyProperties();
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

        [DataSourceProperty]
        public string SendMessengerActionName
        {
            get
            {
                return this._sendMessengerActionName;
            }
            set
            {
                if (value != this._sendMessengerActionName)
                {
                    this._sendMessengerActionName = value;
                    base.OnPropertyChanged("SendMessengerActionName");
                }
            }
        }

        private bool _isOptionAvailable;
        private int _influenceCost;
        private bool _isMessengerAvailable;
        private int _sendMessengerInfluenceCost;
        private string _sendMessengerActionName;
    }
}
