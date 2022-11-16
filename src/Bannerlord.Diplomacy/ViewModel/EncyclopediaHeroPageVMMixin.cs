using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using Diplomacy.Costs;
using Diplomacy.Event;
using Diplomacy.GauntletInterfaces;
using Diplomacy.GrantFief;
using Diplomacy.Messengers;

using JetBrains.Annotations;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;

namespace Diplomacy.ViewModel
{
    [ViewModelMixin(nameof(EncyclopediaHeroPageVM.RefreshValues))]
    class EncyclopediaHeroPageVMMixin : BaseViewModelMixin<EncyclopediaHeroPageVM>
    {
        private bool _isMessengerAvailable;
        private bool _canGrantFief;
        private readonly Hero _hero;
        private readonly DiplomacyCost _sendMessengerCost;
        private readonly GrantFiefInterface _grantFiefInterface;

        private static readonly TextObject _TSendMessengerText = new("{=cXfcwzPp}Send Messenger");
        private static readonly TextObject _TGrantFiefText = new("{=LpoyhORp}Grant Fief");

        public EncyclopediaHeroPageVMMixin(EncyclopediaHeroPageVM vm) : base(vm)
        {
            _grantFiefInterface = new GrantFiefInterface();
            _hero = (vm.Obj as Hero)!;
            _sendMessengerCost = DiplomacyCostCalculator.DetermineCostForSendingMessenger();
            SendMessengerCost = (int) _sendMessengerCost.Value;
            SendMessengerActionName = _TSendMessengerText.ToString();
            GrantFiefActionName = _TGrantFiefText.ToString();
            vm.RefreshValues();
        }

        public override void OnRefresh()
        {
            //ViewModel?.RefreshValues();

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

        [DataSourceMethod]
        public void SendMessenger()
        {

            Events.Instance.OnMessengerSent(_hero);
            OnRefresh();
        }

        //[UsedImplicitly]
        //private new void ExecuteLink(string link)
        //{
        //    Campaign.Current.EncyclopediaManager.GoToLink(link);
        //}

        [DataSourceMethod]
        public void GrantFief()
        {
            _grantFiefInterface.ShowFiefInterface(ScreenManager.TopScreen, _hero, OnRefresh);
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
                    ViewModel?.OnPropertyChanged(nameof(CanGrantFief));
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
                    ViewModel?.OnPropertyChanged(nameof(IsMessengerAvailable));
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