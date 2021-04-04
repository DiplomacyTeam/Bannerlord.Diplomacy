using Diplomacy.DiplomaticAction.Usurp;
using Diplomacy.GauntletInterfaces;
using Diplomacy.GrantFief;
using System;
using System.ComponentModel;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomClan;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    class KingdomClanVMExtensionVM : KingdomClanVM, INotifyPropertyChanged, ICloseableVM
    {
        private readonly Action _executeExpel;
        private readonly Action _executeSupport;

        private bool _canGrantFiefToClan;
        private GrantFiefInterface _grantFiefInterface;
        private HintViewModel? _grantFiefHint;

        private bool _canUsurpThrone;
        private bool _showUsurpThrone;

        private int _usurpThroneInfluenceCost;
        private string _usurpThroneExplanationText;
        private HintViewModel? _usurpThroneHint;

        public KingdomClanVMExtensionVM(Action<TaleWorlds.CampaignSystem.Election.KingdomDecision> forceDecide) : base(forceDecide)
        {
            // FIXME: convert to cached delegates
            _executeExpel = () => typeof(KingdomClanVM).GetMethod("ExecuteExpelCurrentClan", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, null);
            _executeSupport = () => typeof(KingdomClanVM).GetMethod("ExecuteSupport", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, null);

            Events.FiefGranted.AddNonSerializedListener(this, RefreshCanGrantFief);

            _grantFiefInterface = new GrantFiefInterface();
            GrantFiefActionName = new TextObject("{=LpoyhORp}Grant Fief").ToString();
            GrantFiefExplanationText = new TextObject("{=98hwXUTp}Grant fiefs to clans in your kingdom").ToString();
            
            DonateGoldActionName = new TextObject("{=Gzq6VHPt}Donate Gold").ToString();
            DonateGoldExplanationText = new TextObject("{=7QvXkcxH}Donate gold to clans in your kingdom").ToString();
            
            UsurpThroneActionName = new TextObject("{=N7goPgiq}Usurp Throne").ToString();
            _usurpThroneExplanationText = string.Empty;
            
            PropertyChangedWithValue += new PropertyChangedWithValueEventHandler(OnPropertyChangedWithValue);
            
            RefreshCanGrantFief();
            RefreshCanUsurpThrone();
        }

        public void OnClose() => Events.RemoveListeners(this);

        private void ExecuteExpelCurrentClan() => _executeExpel();

        private void ExecuteSupport() => _executeSupport();

        private void OnPropertyChangedWithValue(object sender, PropertyChangedWithValueEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentSelectedClan))
            {
                RefreshCanGrantFief();
                RefreshCanUsurpThrone();
            }
        }

        private void RefreshCanGrantFief(Town town)
        {
            RefreshClan();
            RefreshCanGrantFief();
        }

        private void RefreshCanGrantFief()
        {
            CanGrantFiefToClan = GrantFiefAction.CanGrantFief(CurrentSelectedClan.Clan, out var hint);
            GrantFiefHint = CanGrantFiefToClan ? new HintViewModel() : Compat.HintViewModel.Create(new TextObject(hint));
        }

        private void RefreshCanUsurpThrone()
        {
            ShowUsurpThrone = CurrentSelectedClan.Clan == Clan.PlayerClan;
            CanUsurpThrone = UsurpKingdomAction.CanUsurp(Clan.PlayerClan, out var errorMessage);
            UsurpThroneHint = errorMessage is not null ? Compat.HintViewModel.Create(new TextObject(errorMessage)) : new HintViewModel();

            UsurpKingdomAction.GetClanSupport(Clan.PlayerClan, out var supportingClanTiers, out var opposingClanTiers);

            var textObject = new TextObject("{=WVe7QwhW}Usurp the throne of this kingdom\nClan Support: {SUPPORTING_TIERS} / {OPPOSING_TIERS}");
            textObject.SetTextVariable("SUPPORTING_TIERS", supportingClanTiers);
            textObject.SetTextVariable("OPPOSING_TIERS", opposingClanTiers + 1);

            UsurpThroneExplanationText = textObject.ToString();
            UsurpThroneInfluenceCost = (int)UsurpKingdomAction.GetUsurpInfluenceCost(Clan.PlayerClan);
        }

        public void UsurpThrone()
        {
            UsurpKingdomAction.Apply(Clan.PlayerClan); // FIXME: DIIIIIIIIIIE! :)
            RefreshClan();
        }

        public void GrantFief() => _grantFiefInterface.ShowFiefInterface(ScreenManager.TopScreen, CurrentSelectedClan.Clan.Leader);

        private void DonateGold() => new DonateGoldInterface().ShowInterface(ScreenManager.TopScreen, CurrentSelectedClan.Clan);

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
                    OnPropertyChanged(nameof(CanGrantFiefToClan));
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel GrantFiefHint
        {
            get => _grantFiefHint;
            set
            {
                if (value != _grantFiefHint)
                {
                    _grantFiefHint = value;
                    OnPropertyChanged(nameof(GrantFiefHint));
                }
            }
        }

        [DataSourceProperty]
        public string GrantFiefExplanationText { get; }

        [DataSourceProperty]
        public string DonateGoldActionName { get; }

        [DataSourceProperty]
        public string DonateGoldExplanationText { get; }

        [DataSourceProperty]
        public string UsurpThroneActionName { get; }

        [DataSourceProperty]
        public string UsurpThroneExplanationText
        {
            get => _usurpThroneExplanationText;
            set
            {
                if (value != _usurpThroneExplanationText)
                {
                    _usurpThroneExplanationText = value;
                    OnPropertyChanged(nameof(UsurpThroneExplanationText));
                }
            }

        }

        [DataSourceProperty]
        public bool ShowUsurpThrone
        {
            get => Settings.Instance!.EnableUsurpThroneFeature && _showUsurpThrone;

            set
            {
                if (value != _showUsurpThrone)
                {
                    _showUsurpThrone = value;
                    OnPropertyChanged(nameof(ShowUsurpThrone));
                }
            }

        }

        [DataSourceProperty]
        public bool CanUsurpThrone
        {
            get => _canUsurpThrone;
            set
            {
                if (value != _canUsurpThrone)
                {
                    _canUsurpThrone = value;
                    OnPropertyChanged(nameof(CanUsurpThrone));
                }
            }
        }

        [DataSourceProperty]
        public int UsurpThroneInfluenceCost
        {
            get => _usurpThroneInfluenceCost;
            set
            {
                if (value != _usurpThroneInfluenceCost)
                {
                    _usurpThroneInfluenceCost = value;
                    OnPropertyChanged(nameof(UsurpThroneInfluenceCost));
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel UsurpThroneHint
        {
            get => _usurpThroneHint;
            set
            {
                if (value != _usurpThroneHint)
                {
                    _usurpThroneHint = value;
                    OnPropertyChanged(nameof(UsurpThroneHint));
                }
            }
        }
    }
}
