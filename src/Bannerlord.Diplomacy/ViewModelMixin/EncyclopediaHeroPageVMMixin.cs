using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using Diplomacy.Actions;
using Diplomacy.Costs;
using Diplomacy.Events;
using Diplomacy.GauntletInterfaces;
using Diplomacy.Messengers;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;

namespace Diplomacy.ViewModelMixin
{
    [ViewModelMixin(nameof(EncyclopediaHeroPageVM.RefreshValues))]
    internal sealed class EncyclopediaHeroPageVMMixin : BaseViewModelMixin<EncyclopediaHeroPageVM>
    {
        private bool _isMessengerAvailable;
        private bool _canGrantFief;
        private HintViewModel? _sendMessengerHint;
        private readonly Hero _hero;
        private readonly GoldCost _sendMessengerCost;
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
            DiplomacyEvents.Instance.OnMessengerSent(_hero);
            OnRefresh();
        }

        [DataSourceMethod]
        public void GrantFief()
        {
            _grantFiefInterface.ShowFiefInterface(ScreenManager.TopScreen, _hero, OnRefresh);
        }

        [DataSourceProperty]
        public bool CanGrantFief { get => _canGrantFief; set => SetField(ref _canGrantFief, value, nameof(CanGrantFief)); }

        [DataSourceProperty]
        public int SendMessengerCost { get; }

        [DataSourceProperty]
        public bool IsMessengerAvailable { get => _isMessengerAvailable; set => SetField(ref _isMessengerAvailable, value, nameof(IsMessengerAvailable)); }

        private void UpdateIsMessengerAvailable()
        {
            IsMessengerAvailable = MessengerManager.CanSendMessengerWithCost(_hero, _sendMessengerCost, out var exception);
            SendMessengerHint = !IsMessengerAvailable ? Compat.HintViewModel.Create(exception) : new HintViewModel();
        }

        [DataSourceProperty]
        public string SendMessengerActionName { get; }

        [DataSourceProperty]
        public HintViewModel? SendMessengerHint { get => _sendMessengerHint; set => SetField(ref _sendMessengerHint, value, nameof(SendMessengerHint)); }

        [DataSourceProperty]
        public string GrantFiefActionName { get; }
    }
}