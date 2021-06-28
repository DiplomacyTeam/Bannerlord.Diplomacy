using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using Diplomacy.Event;
using Diplomacy.GauntletInterfaces;
using Diplomacy.GrantFief;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomClan;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModelMixins
{
    [ViewModelMixin]
    internal sealed class KingdomClanVMMixin : BaseViewModelMixin<KingdomClanVM>
    {

        private bool _canGrantFiefToClan;
        private GrantFiefInterface _grantFiefInterface;
        private HintViewModel? _grantFiefHint;

        public KingdomClanVMMixin(KingdomClanVM vm) : base(vm)
        {
            Events.FiefGranted.AddNonSerializedListener(this, RefreshCanGrantFief);

            _grantFiefInterface = new GrantFiefInterface();
            GrantFiefActionName = new TextObject("{=LpoyhORp}Grant Fief").ToString();
            GrantFiefExplanationText = new TextObject("{=98hwXUTp}Grant fiefs to clans in your kingdom").ToString();

            DonateGoldActionName = new TextObject("{=Gzq6VHPt}Donate Gold").ToString();
            DonateGoldExplanationText = new TextObject("{=7QvXkcxH}Donate gold to clans in your kingdom").ToString();

            ViewModel!.PropertyChangedWithValue += new PropertyChangedWithValueEventHandler(OnPropertyChangedWithValue);

            RefreshCanGrantFief();
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            Events.RemoveListeners(this);
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
        public void GrantFief() => _grantFiefInterface.ShowFiefInterface(ScreenManager.TopScreen, ViewModel!.CurrentSelectedClan.Clan.Leader);

        [DataSourceMethod]
        public void DonateGold() => new DonateGoldInterface().ShowInterface(ScreenManager.TopScreen, ViewModel!.CurrentSelectedClan.Clan);

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
