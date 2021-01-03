using Diplomacy.GauntletInterfaces;
using Diplomacy.GrantFief;
using Diplomacy.Messengers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    class EncyclopediaHeroPageVMExtensionVM : EncyclopediaHeroPageVM
    {
        private bool _isMessengerAvailable;
        private bool _canGrantFief;
        private readonly Hero _hero;
        private readonly DiplomacyCost _sendMessengerCost;
        private GrantFiefInterface _grantFiefInterface;

        public EncyclopediaHeroPageVMExtensionVM(EncyclopediaPageArgs args) : base(args)
        {
            _grantFiefInterface = new GrantFiefInterface();
            _hero = (Obj as Hero);
            _sendMessengerCost = DiplomacyCostCalculator.DetermineCostForSendingMessenger();
            SendMessengerCost = (int)_sendMessengerCost.Value;
            SendMessengerActionName = new TextObject("{=cXfcwzPp}Send Messenger").ToString();
            GrantFiefActionName = new TextObject("{=LpoyhORp}Grant Fief").ToString();
            base.RefreshValues();
            Recalculate();
        }

        private void Recalculate()
        {
            if (_hero.Clan?.Kingdom is not null && Clan.PlayerClan?.Kingdom is not null && _hero.Clan.Kingdom == Clan.PlayerClan.Kingdom)
            {
                CanGrantFief = GrantFiefAction.CanGrantFief(_hero.Clan, out _);
            }
            RefreshValues();
        }

        protected void SendMessenger()
        {
            Events.Instance.OnMessengerSent(_hero);
            UpdateIsMessengerAvailable();
        }

        private void ExecuteLink(string link)
        {
            Campaign.Current.EncyclopediaManager.GoToLink(link);
        }

        private void GrantFief()
        {
            _grantFiefInterface.ShowFiefInterface(ScreenManager.TopScreen, _hero, Recalculate);
        }

        [DataSourceProperty]
        public bool CanGrantFief
        {
            get
            {
                return _canGrantFief;
            }
            set
            {
                if (value != _canGrantFief)
                {
                    _canGrantFief = value;
                    OnPropertyChanged("CanGrantFief");
                }
            }
        }

        [DataSourceProperty]
        public int SendMessengerCost { get; }

        [DataSourceProperty]
        public bool SendMessengerCostTypeIsGold { get; }

        [DataSourceProperty]
        public bool IsMessengerAvailable
        {
            get
            {
                UpdateIsMessengerAvailable();
                return _isMessengerAvailable;
            }
            set
            {
                if (value != _isMessengerAvailable)
                {
                    _isMessengerAvailable = value;
                    OnPropertyChanged("IsMessengerAvailable");
                }
            }
        }

        private void UpdateIsMessengerAvailable()
        {
            IsMessengerAvailable = MessengerManager.CanSendMessengerWithCost(_hero, _sendMessengerCost);
        }

        [DataSourceProperty]
        public string SendMessengerActionName { get; private set; }

        [DataSourceProperty]
        public string GrantFiefActionName { get; private set; }
    }
}
