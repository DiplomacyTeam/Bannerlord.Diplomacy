using DiplomacyFixes.Messengers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.ViewModel
{
    class EncyclopediaHeroPageVMExtensionVM : EncyclopediaHeroPageVM
    {
        private int _sendMessengerInfluenceCost;
        private bool _isMessengerAvailable;
        private string _sendMessengerActionName;
        private readonly Hero _hero;

        public EncyclopediaHeroPageVMExtensionVM(EncyclopediaPageArgs args) : base(args)
        {
            _hero = (base.Obj as Hero);
            float sendMessengerInfluenceCost = DiplomacyCostCalculator.DetermineInfluenceCostForSendingMessenger();
            this.SendMessengerInfluenceCost = (int)sendMessengerInfluenceCost;
            this.SendMessengerActionName = new TextObject("{=cXfcwzPp}Send Messenger").ToString();
        }

        protected void SendMessenger()
        {
            Events.Instance.OnMessengerSent(_hero);
            UpdateSendMessengerInfluenceCost();
            UpdateIsMessengerAvailable();
        }

        [DataSourceProperty]
        public int SendMessengerInfluenceCost
        {
            get
            {
                UpdateSendMessengerInfluenceCost();
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

        private void UpdateSendMessengerInfluenceCost()
        {
            SendMessengerInfluenceCost = Settings.Instance.SendMessengerInfluenceCost;
        }

        [DataSourceProperty]
        public bool IsMessengerAvailable
        {
            get
            {
                UpdateIsMessengerAvailable();
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

        private void UpdateIsMessengerAvailable()
        {
            IsMessengerAvailable = MessengerManager.CanSendMessengerWithInfluenceCost(this._hero, SendMessengerInfluenceCost);
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
    }
}
