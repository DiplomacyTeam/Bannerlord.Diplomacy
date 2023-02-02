using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using Diplomacy.Actions;
using Diplomacy.Events;
using Diplomacy.GauntletInterfaces;

using JetBrains.Annotations;

using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Clans;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;

namespace Diplomacy.ViewModelMixin
{
    [ViewModelMixin]
    [UsedImplicitly]
    internal sealed class KingdomClanVMMixin : BaseViewModelMixin<KingdomClanVM>
    {

        private bool _canGrantFiefToClan;
        private readonly GrantFiefInterface _grantFiefInterface;
        private readonly DonateGoldInterface _donateGoldInterface;
        private HintViewModel? _grantFiefHint;
        private readonly PropertyChangedWithValueEventHandler _eventHandler;

        public KingdomClanVMMixin(KingdomClanVM vm) : base(vm)
        {
            DiplomacyEvents.FiefGranted.AddNonSerializedListener(this, RefreshCanGrantFief);

            _grantFiefInterface = new GrantFiefInterface();
            _donateGoldInterface = new DonateGoldInterface();
            GrantFiefActionName = new TextObject("{=LpoyhORp}Grant Fief").ToString();
            GrantFiefExplanationText = new TextObject("{=98hwXUTp}Grant fiefs to clans in your kingdom").ToString();

            DonateGoldActionName = new TextObject("{=Gzq6VHPt}Donate Gold").ToString();
            DonateGoldExplanationText = new TextObject("{=7QvXkcxH}Donate gold to clans in your kingdom").ToString();

            _eventHandler = new PropertyChangedWithValueEventHandler(OnPropertyChangedWithValue);

            ViewModel!.PropertyChangedWithValue += _eventHandler;

            RefreshCanGrantFief();
        }


        public override void OnFinalize()
        {
            base.OnFinalize();
            DiplomacyEvents.RemoveListeners(this);
            ViewModel!.PropertyChangedWithValue -= _eventHandler;
        }

        private void OnPropertyChangedWithValue(object sender, PropertyChangedWithValueEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.CurrentSelectedClan))
            {
                RefreshCanGrantFief();
            }
        }

        private void RefreshCanGrantFief(Town town)
        {
            ViewModel!.RefreshClan();
            RefreshCanGrantFief();
        }

        private void RefreshCanGrantFief()
        {
            CanGrantFiefToClan = GrantFiefAction.CanGrantFief(ViewModel!.CurrentSelectedClan.Clan, out var hint);
            GrantFiefHint = CanGrantFiefToClan ? new HintViewModel() : Compat.HintViewModel.Create(new TextObject(hint));
        }

        [DataSourceMethod]
        [UsedImplicitly]
        public void GrantFief() => _grantFiefInterface.ShowFiefInterface(ScreenManager.TopScreen, ViewModel!.CurrentSelectedClan.Clan.Leader);

        [DataSourceMethod]
        [UsedImplicitly]
        public void DonateGold() => _donateGoldInterface.ShowInterface(ScreenManager.TopScreen, ViewModel!.CurrentSelectedClan.Clan);

        [DataSourceProperty]
        public string GrantFiefActionName { get; }

        [DataSourceProperty]
        public bool CanGrantFiefToClan
        {
            get => _canGrantFiefToClan;
            set
            {
                if (value != _canGrantFiefToClan)
                {
                    _canGrantFiefToClan = value;
                    ViewModel!.OnPropertyChanged(nameof(CanGrantFiefToClan));
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel GrantFiefHint
        {
            get => _grantFiefHint!;
            set
            {
                if (value != _grantFiefHint)
                {
                    _grantFiefHint = value;
                    ViewModel!.OnPropertyChanged(nameof(GrantFiefHint));
                }
            }
        }

        [DataSourceProperty]
        public string GrantFiefExplanationText { get; }

        [DataSourceProperty]
        public string DonateGoldActionName { get; }

        [DataSourceProperty]
        public string DonateGoldExplanationText { get; }
    }
}