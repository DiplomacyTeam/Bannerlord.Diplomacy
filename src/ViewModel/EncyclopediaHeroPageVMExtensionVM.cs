using Diplomacy.Event;
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
        private readonly GrantFiefInterface _grantFiefInterface;

        private static readonly TextObject _TSendMessengerText = new("{=cXfcwzPp}Send Messenger");
        private static readonly TextObject _TGrantFiefText = new("{=LpoyhORp}Grant Fief");

        public EncyclopediaHeroPageVMExtensionVM(EncyclopediaPageArgs args) : base(args)
        {
            _grantFiefInterface = new GrantFiefInterface();
            _hero = (Obj as Hero)!;
            _sendMessengerCost = DiplomacyCostCalculator.DetermineCostForSendingMessenger();
            SendMessengerCost = (int)_sendMessengerCost.Value;
            SendMessengerActionName = _TSendMessengerText.ToString();
            GrantFiefActionName = _TGrantFiefText.ToString();
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            
            // this is called before the constructor the first time
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (_hero is null)
            {
                return;
            }

            if (_hero.Clan?.Kingdom is not null && Clan.PlayerClan?.Kingdom is not null && _hero.Clan.Kingdom == Clan.PlayerClan.Kingdom)
            {
                CanGrantFief = GrantFiefAction.CanGrantFief(_hero.Clan, out _);
            }

            UpdateIsMessengerAvailable();
        }

        protected void SendMessenger()
        {
            Events.Instance.OnMessengerSent(_hero);
            RefreshValues();
        }

        private void ExecuteLink(string link)
        {
            Campaign.Current.EncyclopediaManager.GoToLink(link);
        }

        private void GrantFief()
        {
            _grantFiefInterface.ShowFiefInterface(ScreenManager.TopScreen, _hero, RefreshValues);
        }

        [DataSourceProperty]
        public bool CanGrantFief
        {
            get => _canGrantFief;
            set
            {
                if (value != _canGrantFief)
                {
                    _canGrantFief = value;
                    OnPropertyChanged(nameof(CanGrantFief));
                }
            }
        }

        [DataSourceProperty]
        public int SendMessengerCost { get; }

        [DataSourceProperty]
        public bool IsMessengerAvailable
        {
            get => _isMessengerAvailable;
            set
            {
                if (value != _isMessengerAvailable)
                {
                    _isMessengerAvailable = value;
                    OnPropertyChanged(nameof(IsMessengerAvailable));
                }
            }
        }

        private void UpdateIsMessengerAvailable()
        {
            IsMessengerAvailable = MessengerManager.CanSendMessengerWithCost(_hero, _sendMessengerCost);
        }

        [DataSourceProperty]
        public string SendMessengerActionName { get; }

        [DataSourceProperty]
        public string GrantFiefActionName { get; }
    }
}
