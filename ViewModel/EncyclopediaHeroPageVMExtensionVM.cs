using DiplomacyFixes.GauntletInterfaces;
using DiplomacyFixes.Messengers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.ViewModel
{
    class EncyclopediaHeroPageVMExtensionVM : EncyclopediaHeroPageVM
    {
        private int _sendMessengerInfluenceCost;
        private bool _isMessengerAvailable;
        private string _sendMessengerActionName;
        private bool _canGrantFief;
        private string _grantFiefActionName;
        private readonly Hero _hero;

        private GrantFiefInterface _grantFiefInterface;

        public EncyclopediaHeroPageVMExtensionVM(EncyclopediaPageArgs args) : base(args)
        {
            this._grantFiefInterface = new GrantFiefInterface();
            _hero = (base.Obj as Hero);
            float sendMessengerInfluenceCost = DiplomacyCostCalculator.DetermineInfluenceCostForSendingMessenger();
            this.SendMessengerInfluenceCost = (int)sendMessengerInfluenceCost;
            this.SendMessengerActionName = new TextObject("{=cXfcwzPp}Send Messenger").ToString();
            this.GrantFiefActionName = new TextObject("{=LpoyhORp}Grant Fief").ToString();

            if (_hero.Clan != Clan.PlayerClan && _hero.MapFaction.Leader == Hero.MainHero && _hero.MapFaction is Kingdom)
            {
                CanGrantFief = true;
            }
            else
            {
                CanGrantFief = false;
            }
            base.RefreshValues();
        }

        protected void SendMessenger()
        {
            Events.Instance.OnMessengerSent(_hero);
            UpdateSendMessengerInfluenceCost();
            UpdateIsMessengerAvailable();
        }

        private void ExecuteLink(string link)
        {
            Campaign.Current.EncyclopediaManager.GoToLink(link);
        }

        private void GrantFief()
        {
            _grantFiefInterface.ShowFiefInterface(ScreenManager.TopScreen, this._hero);
        }

        [DataSourceProperty]
        public bool CanGrantFief
        {
            get
            {
                return this._canGrantFief;
            }
            set
            {
                if (value != this._canGrantFief)
                {
                    this._canGrantFief = value;
                    base.OnPropertyChanged("CanGrantFief");
                }
            }
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

        [DataSourceProperty]
        public string GrantFiefActionName
        {
            get
            {
                return this._grantFiefActionName;
            }
            set
            {
                if (value != this._grantFiefActionName)
                {
                    this._grantFiefActionName = value;
                    base.OnPropertyChanged("GrantFiefActionName");
                }
            }
        }
    }
}
