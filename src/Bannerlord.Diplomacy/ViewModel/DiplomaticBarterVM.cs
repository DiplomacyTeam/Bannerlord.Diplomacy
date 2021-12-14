using System;
using System.Collections.Generic;
using System.Linq;
using Diplomacy.DiplomaticAction.Barter;
using Diplomacy.DiplomaticAction.Barter.Barterables;
using Diplomacy.Extensions;
using JetBrains.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    internal sealed class DiplomaticBarterVM : TaleWorlds.Library.ViewModel
    {
        private static readonly TextObject _TProposeLabel = new("{=}Propose");
        private static readonly TextObject _TDemandLabel = new("{=}Declare");
        private static readonly TextObject _TAlliesLabel = new("{=}Allies");
        private static readonly TextObject _TWarsLabel = new("{=}Wars");
        private static readonly TextObject _TPactsLabel = new("{=}Non-Aggression Pacts");
        private static readonly TextObject _TOurTreatiesLabel = new("{=}Our Treaties");
        private static readonly TextObject _TTheirTreatiesLabel = new("{=}Their Treaties");
        private static readonly TextObject _TOurContributionsLabel = new("{=}Our Contributions");
        private static readonly TextObject _TTheirContributionsLabel = new("{=}Their Contributions");
        private static readonly TextObject _TMutualContributionsLabel = new("{=}Mutual Contributions");
        private static readonly TextObject _TDurationLabel = new("{=}Duration (in days)");
        private static readonly TextObject _TAmountLabel = new("{=}Payment Amount");

        private readonly List<DiplomaticBarterFactionVM> _availableFactions;
        private readonly IFaction _faction1;
        private readonly Action _onFinalize;
        private MBBindingList<DiplomaticBarterItemVM> _agreementOptions;
        private float _barterBalance;
        private DiplomaticBarterItemVM _currentPropertyChangeItem = null!;
        private DiplomaticBarterFactionVM _currentSelectedFaction = null!;
        private MBBindingList<DiplomaticBarterItemVM> _faction1Contributions;
        private ImageIdentifierVM _faction1Visual;
        private IFaction _faction2;
        private MBBindingList<DiplomaticBarterItemVM> _faction2Contributions;
        private ImageIdentifierVM _faction2Visual;
        private SelectorVM<DiplomaticBarterFactionVM> _factionSelection = null!;
        private HeroViewModel _heroCharacter = null!;
        private bool _isNotAcceptable;
        private MBBindingList<DiplomaticBarterItemVM> _mutualContributions;
        private float _negativeBarterBalance;
        private float _positiveBarterBalance;
        private string _proposeLabel = null!;
        private bool _showPropertyChange;
        private int _totalInfluenceCost;

        [DataSourceProperty]
        public HeroViewModel HeroCharacter
        {
            get => _heroCharacter;
            set
            {
                if (value != _heroCharacter)
                {
                    _heroCharacter = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM Faction1Visual
        {
            get => _faction1Visual;
            set
            {
                _faction1Visual = value;
                OnPropertyChanged(nameof(Faction1Visual));
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM Faction2Visual
        {
            get => _faction2Visual;
            set
            {
                _faction2Visual = value;
                OnPropertyChanged(nameof(Faction2Visual));
            }
        }

        [DataSourceProperty]
        [UsedImplicitly]
        public MBBindingList<DiplomaticBarterItemVM> AgreementOptions
        {
            get => _agreementOptions;
            set
            {
                _agreementOptions = value;
                OnPropertyChanged(nameof(AgreementOptions));
            }
        }

        [DataSourceProperty]
        [UsedImplicitly]
        public MBBindingList<DiplomaticBarterItemVM> Faction1Contributions
        {
            get => _faction1Contributions;
            set
            {
                _faction1Contributions = value;
                OnPropertyChanged(nameof(Faction1Contributions));
            }
        }

        [DataSourceProperty]
        [UsedImplicitly]
        public MBBindingList<DiplomaticBarterItemVM> Faction2Contributions
        {
            get => _faction2Contributions;
            set
            {
                _faction2Contributions = value;
                OnPropertyChanged(nameof(Faction2Contributions));
            }
        }

        [DataSourceProperty]
        [UsedImplicitly]
        public MBBindingList<DiplomaticBarterItemVM> MutualContributions
        {
            get => _mutualContributions;
            set
            {
                _mutualContributions = value;
                OnPropertyChanged(nameof(MutualContributions));
            }
        }

        [DataSourceProperty]
        public bool IsNotAcceptable
        {
            get => _isNotAcceptable;
            set
            {
                _isNotAcceptable = value;
                OnPropertyChanged(nameof(IsNotAcceptable));
            }
        }

        [DataSourceProperty]
        public DiplomaticBarterFactionVM CurrentSelectedFaction
        {
            get => _currentSelectedFaction;
            set
            {
                _currentSelectedFaction = value;
                OnPropertyChanged(nameof(CurrentSelectedFaction));
            }
        }

        [DataSourceProperty]
        public SelectorVM<DiplomaticBarterFactionVM> FactionSelection
        {
            get => _factionSelection;
            set
            {
                _factionSelection = value;
                OnPropertyChanged(nameof(FactionSelection));
            }
        }

        [DataSourceProperty]
        public string ProposeLabel
        {
            get => _proposeLabel;
            set
            {
                _proposeLabel = value;
                OnPropertyChanged(nameof(ProposeLabel));
            }
        }

        [DataSourceProperty]
        public int TotalInfluenceCost
        {
            get => _totalInfluenceCost;
            set
            {
                _totalInfluenceCost = value;
                OnPropertyChanged(nameof(TotalInfluenceCost));
            }
        }

        [DataSourceProperty] public float BarterMaxBalance => 20f;
        [DataSourceProperty] public float BarterMinBalance => 0f;

        [DataSourceProperty]
        public float NegativeBarterBalance
        {
            get => _negativeBarterBalance;
            set
            {
                _negativeBarterBalance = value;
                OnPropertyChanged(nameof(NegativeBarterBalance));
            }
        }

        [DataSourceProperty]
        public float PositiveBarterBalance
        {
            get => _positiveBarterBalance;
            set
            {
                _positiveBarterBalance = value;
                OnPropertyChanged(nameof(PositiveBarterBalance));
            }
        }


        [DataSourceProperty]
        public bool ShowPropertyChange
        {
            get => _showPropertyChange;
            set
            {
                _showPropertyChange = value;
                OnPropertyChanged(nameof(ShowPropertyChange));
            }
        }

        [DataSourceProperty]
        public DiplomaticBarterItemVM CurrentPropertyChangeItem
        {
            get => _currentPropertyChangeItem;
            set
            {
                _currentPropertyChangeItem = value;
                OnPropertyChanged(nameof(CurrentPropertyChangeItem));
            }
        }

        [DataSourceProperty]
        public float BarterBalance
        {
            get => _barterBalance;
            set
            {
                _barterBalance = value;
                OnPropertyChanged(nameof(BarterBalance));
            }
        }

        [DataSourceProperty] public DiplomaticBarterFactionVM PlayerFaction { get; }

        [DataSourceProperty] public string PactsLabel => _TPactsLabel.ToString();
        [DataSourceProperty] public string AlliesLabel => _TAlliesLabel.ToString();
        [DataSourceProperty] public string WarsLabel => _TWarsLabel.ToString();
        [DataSourceProperty] public string OurTreatiesLabel => _TOurTreatiesLabel.ToString();
        [DataSourceProperty] public string TheirTreatiesLabel => _TTheirTreatiesLabel.ToString();
        [DataSourceProperty] public string OurContributionsLabel => _TOurContributionsLabel.ToString();
        [DataSourceProperty] public string TheirContributionsLabel => _TTheirContributionsLabel.ToString();
        [DataSourceProperty] public string MutualContributionsLabel => _TMutualContributionsLabel.ToString();
        [DataSourceProperty] public string CancelLabel => GameTexts.FindText("str_cancel").ToString();
        [DataSourceProperty] public string AmountLabel => _TAmountLabel.ToString();
        [DataSourceProperty] public string DurationLabel => _TDurationLabel.ToString();

        private DiplomaticBarter _diplomaticBarter;
        private bool _showBalanceBarterInPropertyChange;

        public DiplomaticBarterVM(IFaction faction1, IFaction faction2, Action onFinalize)
        {
            _onFinalize = onFinalize;

            _faction1 = faction1;
            _faction2 = faction2;
            _faction1Visual = new ImageIdentifierVM(BannerCode.CreateFrom(faction1.Banner), true);
            _faction2Visual = new ImageIdentifierVM(BannerCode.CreateFrom(faction2.Banner), true);


            _faction1Contributions = new MBBindingList<DiplomaticBarterItemVM>();
            _faction2Contributions = new MBBindingList<DiplomaticBarterItemVM>();
            _mutualContributions = new MBBindingList<DiplomaticBarterItemVM>();
            _agreementOptions = new MBBindingList<DiplomaticBarterItemVM>();

            HeroCharacter = new HeroViewModel(CharacterViewModel.StanceTypes.EmphasizeFace);
            PlayerFaction = new DiplomaticBarterFactionVM(faction1);

            // CampaignEvents.WarDeclared.AddNonSerializedListener(this, (_, _) => Refresh());
            _availableFactions = new List<DiplomaticBarterFactionVM>();

            UpdateHeroCharacter();

            _diplomaticBarter = new(faction1, faction2);

            Refresh();
        }

        private void OnShowPropertyChange(DiplomaticBarterItemVM barterItem)
        {
            CurrentPropertyChangeItem = barterItem;
            ShowBalanceBarterInPropertyChange = barterItem.Item is PaymentBarterable;
            ShowPropertyChange = true;
        }

        [UsedImplicitly]
        private void OnResetPropertyChange()
        {
            CurrentPropertyChangeItem.NewAmount = CurrentPropertyChangeItem.Amount;
            CurrentPropertyChangeItem.NewDuration = CurrentPropertyChangeItem.Duration;
        }

        [UsedImplicitly]
        private void OnCancelPropertyChange()
        {
            ShowPropertyChange = false;
            CurrentPropertyChangeItem.NewAmount = CurrentPropertyChangeItem.Amount;
            CurrentPropertyChangeItem.NewDuration = CurrentPropertyChangeItem.Duration;
        }

        [UsedImplicitly]
        private void OnApplyPropertyChange()
        {
            CurrentPropertyChangeItem.Amount = CurrentPropertyChangeItem.NewAmount;
            CurrentPropertyChangeItem.Duration = CurrentPropertyChangeItem.NewDuration;
            if (!GetAllProposalItems().Contains(CurrentPropertyChangeItem))
                AddToProposal(CurrentPropertyChangeItem);
            else
                Refresh();
            ShowPropertyChange = false;
        }

        public void Refresh()
        {
            _availableFactions.Clear();
            _availableFactions.AddRange(Kingdom.All.Where(x => x != Clan.PlayerClan.Kingdom && !x.IsRebelKingdom() && !x.IsEliminated)
                .Select(x => new DiplomaticBarterFactionVM(x)).ToList());

            PlayerFaction.Refresh();

            UpdateFactionSelection();

            var netScore = // MBMath.ClampFloat(GetAllContributions().Sum(x => x.GetScore()), -100f, 100f);
                MBMath.ClampFloat(_diplomaticBarter.GetNetScore(), -100f, 100f);

            PositiveBarterBalance = netScore > 0 ? netScore : 0f;
            NegativeBarterBalance = netScore < 0 ? -netScore : 0f;
            BarterBalance = netScore;

            var influenceCost = _diplomaticBarter.GetInfluenceCost();
            TotalInfluenceCost = Convert.ToInt32(influenceCost);

            IsNotAcceptable = !_diplomaticBarter.IsProposalAcceptable();

            AgreementOptions.Clear();

            foreach (var option in DiplomaticBarter.GetBarterItems(_faction1, _faction2))
                AgreementOptions.Add(new DiplomaticBarterItemVM(option, option.IsValid(_diplomaticBarter.Proposal),
                    AddToProposal,
                    RemoveFromProposal,
                    OnShowPropertyChange));

            Faction1Contributions.Clear();
            Faction2Contributions.Clear();
            MutualContributions.Clear();

            foreach (var abstractDiplomaticBarterable in _diplomaticBarter.Proposal)
            {
                var barterItemVM = new DiplomaticBarterItemVM(abstractDiplomaticBarterable, 
                    true, 
                    AddToProposal,
                    RemoveFromProposal, 
                    OnShowPropertyChange);
                switch (abstractDiplomaticBarterable.ContributionParty)
                {
                    case ContributionParty.Mutual:
                        MutualContributions.Add(barterItemVM);
                        break;
                    case ContributionParty.Proposing:
                        Faction1Contributions.Add(barterItemVM);
                        break;
                    case ContributionParty.Considering:
                        Faction2Contributions.Add(barterItemVM);
                        break;
                }
            }

            foreach (var diplomaticBarterItemVM in AgreementOptions) diplomaticBarterItemVM.IsOption = true;

            foreach (var diplomaticBarterItemVM in GetAllProposalItems()) diplomaticBarterItemVM.IsOption = false;

            ProposeLabel = _diplomaticBarter.Proposal.Any(x => x.IsDeclaration)
                ? _TDemandLabel.ToString()
                : _TProposeLabel.ToString();
        }

        private void UpdateFactionSelection(int selectedIndex = 0)
        {
            FactionSelection = new SelectorVM<DiplomaticBarterFactionVM>(0, null);

            foreach (var faction in _availableFactions) FactionSelection.AddItem(faction);
            var startingIndex = _availableFactions.Select(x => x.Faction).ToList().IndexOf(_faction2);
            FactionSelection.SelectedIndex = startingIndex;
            FactionSelection.SetOnChangeAction(FactionChange);
            CurrentSelectedFaction = FactionSelection.GetCurrentItem();
        }

        private void UpdateHeroCharacter()
        {
            HeroCharacter.FillFrom(_faction2.Leader, -1, _faction2.Leader.IsNotable);
            HeroCharacter.SetEquipment(EquipmentIndex.ArmorItemEndSlot, default);
            HeroCharacter.SetEquipment(EquipmentIndex.HorseHarness, default);
            HeroCharacter.SetEquipment(EquipmentIndex.NumAllWeaponSlots, default);
        }

        private void FactionChange(SelectorVM<DiplomaticBarterFactionVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                var factionVM = obj.SelectedItem;
                _faction2 = factionVM.Faction;
                Faction2Visual = factionVM.FactionVisual;
                _diplomaticBarter = new DiplomaticBarter(_faction1, _faction2);
                UpdateHeroCharacter();
                CurrentSelectedFaction = obj.SelectedItem;
                OnReset();
            }
        }

        private void BalanceBarterWithPayment()
        {
            float requiredScore = _diplomaticBarter.Proposal.Where(x => x is not PaymentBarterable).Sum(x => x.GetNetScore());
            if ((CurrentPropertyChangeItem.Item as PaymentBarterable)!.CanGetValueForDiplomaticScore(-requiredScore, out int value, false))
            {
                CurrentPropertyChangeItem.NewAmount = value;
            }
            else
            {
                CurrentPropertyChangeItem.NewAmount = CurrentPropertyChangeItem.MaxAmount;
            }
        }

        [DataSourceProperty]
        public bool ShowBalanceBarterInPropertyChange
        {
            get => _showBalanceBarterInPropertyChange;
            set { _showBalanceBarterInPropertyChange = value; OnPropertyChanged(nameof(ShowBalanceBarterInPropertyChange)); }
        }

        [UsedImplicitly]
        public void AddToProposal(DiplomaticBarterItemVM barterItem)
        {
            _diplomaticBarter.AddToProposal(barterItem.Item);
            Refresh();
        }

        [UsedImplicitly]
        public void RemoveFromProposal(DiplomaticBarterItemVM barterItem)
        {
            _diplomaticBarter.RemoveFromProposal(barterItem.Item);
            Refresh();
        }

        [UsedImplicitly]
        public void ExecuteBarter()
        {
            _diplomaticBarter.ExecuteProposal();
            OnReset();
        }

        private IReadOnlyList<DiplomaticBarterItemVM> GetAllProposalItems()
        {
            return Faction1Contributions.Union(Faction2Contributions).Union(MutualContributions).ToList();
        }

        [UsedImplicitly]
        public void OnComplete()
        {
            HeroCharacter.OnFinalize();
            _onFinalize();
        }

        [UsedImplicitly]
        public void OnReset()
        {
            _diplomaticBarter.Clear();
            Faction1Contributions.Clear();
            Faction2Contributions.Clear();
            MutualContributions.Clear();
            Refresh();
        }
    }
}