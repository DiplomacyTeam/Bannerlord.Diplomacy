using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using Diplomacy.Costs;
using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.DiplomaticAction.NonAggressionPact;
using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.ViewModel;

using JetBrains.Annotations;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModelMixin
{
    [ViewModelMixin("UpdateDiplomacyProperties")]
    [UsedImplicitly]
    internal sealed class KingdomTruceItemVmMixin : BaseViewModelMixin<KingdomTruceItemVM>
    {
        private static readonly TextObject _TFormAlliance = new("{=0WPWbx70}Form Alliance");
        private static readonly TextObject _TFormPact = new("{=9pY0NQrk}Form Pact");
        private static readonly TextObject _TWars = new("{=y5tXjbLK}Wars");
        private static readonly TextObject _TAlliances = new("{=zpNalMeA}Alliances");
        private static readonly TextObject _TPacts = new(StringConstants.NonAggressionPacts);

        private static readonly TextObject _TNapHelpText = new("{=9zlQNtlX}Form a non-aggression pact lasting {DAYS} days.");
        private static readonly TextObject _TWarExhaustion = new("{=XmV_TQ0bH}War Exhaustion");

        private static readonly TextObject _TBreakAlliance = new("{=K4GraLTn}Break Alliance");

        private static readonly TextObject _TPlus = new("{=eTw2aNV5}+");
        private static readonly TextObject _TRequiredScore = new("{=XIBUWDlT}Required Score");
        private static readonly TextObject _TCurrentScore = new("{=5r6fsHgm}Current Score");
        private readonly Kingdom _faction1;
        private readonly Kingdom _faction2;
        private readonly bool _isAlliance;
        private HintViewModel? _actionHint;
        private string _actionName = null!;
        private int _allianceGoldCost;
        private HintViewModel? _allianceHint;
        private int _allianceInfluenceCost;
        private BasicTooltipViewModel? _allianceScoreHint;
        private bool _isAllianceAvailable;
        private bool _isNonAggressionPactAvailable;
        private bool _isOptionAvailable;
        private int _nonAggressionPactGoldCost;
        private HintViewModel? _nonAggressionPactHint;
        private int _nonAggressionPactInfluenceCost;
        private BasicTooltipViewModel? _nonAggressionPactScoreHint;

        [DataSourceProperty]
        public bool IsAllianceAvailable
        {
            get => _isAllianceAvailable;
            set
            {
                if (value != _isAllianceAvailable)
                {
                    _isAllianceAvailable = value;
                    ViewModel!.OnPropertyChanged(nameof(IsAllianceAvailable));
                }
            }
        }

        [DataSourceProperty]
        public int AllianceInfluenceCost
        {
            get => _allianceInfluenceCost;
            set
            {
                if (value != _allianceInfluenceCost)
                {
                    _allianceInfluenceCost = value;
                    ViewModel!.OnPropertyChanged(nameof(AllianceInfluenceCost));
                }
            }
        }

        [DataSourceProperty]
        public int AllianceGoldCost
        {
            get => _allianceGoldCost;
            set
            {
                if (value != _allianceGoldCost)
                {
                    _allianceGoldCost = value;
                    ViewModel!.OnPropertyChanged(nameof(AllianceGoldCost));
                }
            }
        }

        [DataSourceProperty]
        public bool IsNonAggressionPactAvailable
        {
            get => _isNonAggressionPactAvailable;
            set
            {
                if (value != _isNonAggressionPactAvailable)
                {
                    _isNonAggressionPactAvailable = value;
                    ViewModel!.OnPropertyChanged(nameof(IsNonAggressionPactAvailable));
                }
            }
        }

        [DataSourceProperty]
        public int NonAggressionPactInfluenceCost
        {
            get => _nonAggressionPactInfluenceCost;
            set
            {
                if (value != _nonAggressionPactInfluenceCost)
                {
                    _nonAggressionPactInfluenceCost = value;
                    ViewModel!.OnPropertyChanged(nameof(NonAggressionPactInfluenceCost));
                }
            }
        }

        [DataSourceProperty]
        public int NonAggressionPactGoldCost
        {
            get => _nonAggressionPactGoldCost;
            set
            {
                if (value != _nonAggressionPactGoldCost)
                {
                    _nonAggressionPactGoldCost = value;
                    ViewModel!.OnPropertyChanged(nameof(NonAggressionPactGoldCost));
                }
            }
        }

        [DataSourceProperty]
        public string ActionName
        {
            get => _actionName;
            set
            {
                if (value != _actionName)
                {
                    _actionName = value;
                    ViewModel!.OnPropertyChanged(nameof(ActionName));
                }
            }
        }

        [DataSourceProperty] public string NonAggressionPactActionName { get; }

        [DataSourceProperty] public string AllianceText { get; }

        [DataSourceProperty] public string WarsText { get; }

        [DataSourceProperty] public string PactsText { get; }

        [DataSourceProperty] public int InfluenceCost { get; }

        [DataSourceProperty]
        public bool IsOptionAvailable
        {
            get => _isOptionAvailable;
            set
            {
                if (value != _isOptionAvailable)
                {
                    _isOptionAvailable = value;
                    ViewModel!.OnPropertyChanged(nameof(IsOptionAvailable));
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel? ActionHint
        {
            get => _actionHint;
            set
            {
                if (value != _actionHint)
                {
                    _actionHint = value;
                    ViewModel!.OnPropertyChanged(nameof(ActionHint));
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel? AllianceHint
        {
            get => _allianceHint;
            set
            {
                if (value != _allianceHint)
                {
                    _allianceHint = value;
                    ViewModel!.OnPropertyChanged(nameof(AllianceHint));
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel? NonAggressionPactHint
        {
            get => _nonAggressionPactHint;
            set
            {
                if (value != _nonAggressionPactHint)
                {
                    _nonAggressionPactHint = value;
                    ViewModel!.OnPropertyChanged(nameof(NonAggressionPactHint));
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel? AllianceScoreHint
        {
            get => _allianceScoreHint;
            set
            {
                if (value != _allianceScoreHint)
                {
                    _allianceScoreHint = value;
                    ViewModel!.OnPropertyChanged(nameof(AllianceScoreHint));
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel? NonAggressionPactScoreHint
        {
            get => _nonAggressionPactScoreHint;
            set
            {
                if (value != _nonAggressionPactScoreHint)
                {
                    _nonAggressionPactScoreHint = value;
                    ViewModel!.OnPropertyChanged(nameof(NonAggressionPactScoreHint));
                }
            }
        }

        [DataSourceProperty] public string AllianceActionName { get; }

        [DataSourceProperty][UsedImplicitly] public bool IsGoldCostVisible { get; }

        [DataSourceProperty] public string NonAggressionPactHelpText { get; }

        [DataSourceProperty] public DiplomacyPropertiesVM? DiplomacyProperties { get; set; }

        [DataSourceProperty] public bool IsWarItem { get; }

        public KingdomTruceItemVmMixin(KingdomTruceItemVM vm) : base(vm)
        {
            _faction1 = (Kingdom) ViewModel!.Faction1;
            _faction2 = (Kingdom) ViewModel!.Faction2;
            IsWarItem = false;
            AllianceActionName = _TFormAlliance.ToString();
            NonAggressionPactActionName = _TFormPact.ToString();
            AllianceText = _TAlliances.ToString();
            WarsText = _TWars.ToString();
            PactsText = _TPacts.ToString();
            NonAggressionPactHelpText = _TNapHelpText.SetTextVariable("DAYS", Settings.Instance!.NonAggressionPactDuration).ToString();
            _isAlliance = ViewModel!.Faction1.GetStanceWith(ViewModel!.Faction2).IsAllied;
            ActionName = _isAlliance ? _TBreakAlliance.ToString() : GameTexts.FindText("str_kingdom_declate_war_action").ToString();
            InfluenceCost = _isAlliance ? 0 : (int) DiplomacyCostCalculator.DetermineCostForDeclaringWar((Kingdom) ViewModel!.Faction1, true).Value;
            OnRefresh();
        }

        public override void OnRefresh()
        {
            DiplomacyProperties ??= new DiplomacyPropertiesVM(ViewModel!.Faction1, ViewModel!.Faction2);

            DiplomacyProperties.UpdateDiplomacyProperties();
            UpdateActionAvailability();

            if (Settings.Instance!.EnableWarExhaustion)
            {
                if (ViewModel!.Stats.Count > 1 && ViewModel!.Stats[1].Name.Equals(_TWarExhaustion.ToString()))
                    ViewModel!.Stats.RemoveAt(1);

                ViewModel!.Stats.Insert(1, new KingdomWarComparableStatVM(
                    (int) WarExhaustionManager.Instance.GetWarExhaustion(_faction1, _faction2),
                    (int) WarExhaustionManager.Instance.GetWarExhaustion(_faction2, _faction1),
                    _TWarExhaustion,
                    Color.FromUint(ViewModel!.Faction1.Color).ToString(),
                    Color.FromUint(ViewModel!.Faction2.Color).ToString(),
                    100));
            }
        }

        private void UpdateActionAvailability()
        {
            if (_isAlliance)
            {
                var breakAllianceException = BreakAllianceConditions.Instance.CanApplyExceptions(ViewModel!).FirstOrDefault();
                ActionHint = breakAllianceException is not null ? Compat.HintViewModel.Create(breakAllianceException) : new HintViewModel();
                IsOptionAvailable = breakAllianceException is null;
                return;
            }

            IsOptionAvailable = DeclareWarConditions.Instance.CanApplyExceptions(ViewModel!).IsEmpty();

            var allianceException = FormAllianceConditions.Instance.CanApplyExceptions(ViewModel!).FirstOrDefault();
            var declareWarException = DeclareWarConditions.Instance.CanApplyExceptions(ViewModel!).FirstOrDefault();
            var napException = NonAggressionPactConditions.Instance.CanApplyExceptions(ViewModel!).FirstOrDefault();

            IsAllianceAvailable = allianceException is null;
            IsNonAggressionPactAvailable = napException is null;

            ActionHint = declareWarException is not null ? Compat.HintViewModel.Create(declareWarException) : new HintViewModel();
            AllianceHint = allianceException is not null ? Compat.HintViewModel.Create(allianceException) : new HintViewModel();
            NonAggressionPactHint = napException is not null ? Compat.HintViewModel.Create(napException) : new HintViewModel();

            var allianceCost = DiplomacyCostCalculator.DetermineCostForFormingAlliance((Kingdom) ViewModel!.Faction1,
                (Kingdom) ViewModel!.Faction2,
                true);
            AllianceInfluenceCost = (int) allianceCost.InfluenceCost.Value;
            AllianceGoldCost = (int) allianceCost.GoldCost.Value;

            var nonAggressionPactCost = DiplomacyCostCalculator.DetermineCostForFormingNonAggressionPact((Kingdom) ViewModel!.Faction1,
                (Kingdom) ViewModel!.Faction2,
                true);
            NonAggressionPactInfluenceCost = (int) nonAggressionPactCost.InfluenceCost.Value;
            NonAggressionPactGoldCost = (int) nonAggressionPactCost.GoldCost.Value;

            var allianceScore = AllianceScoringModel.Instance.GetScore((Kingdom) ViewModel!.Faction2, (Kingdom) ViewModel!.Faction1, true);
            var napScore = NonAggressionPactScoringModel.Instance.GetScore((Kingdom) ViewModel!.Faction2, (Kingdom) ViewModel!.Faction1, true);
            AllianceScoreHint = UpdateDiplomacyTooltip(allianceScore);
            NonAggressionPactScoreHint = UpdateDiplomacyTooltip(napScore);
        }

        private BasicTooltipViewModel UpdateDiplomacyTooltip(ExplainedNumber explainedNumber)
        {
            static string PlusPrefixed(float value)
            {
                return $"{(value >= 0.005f ? _TPlus.ToString() : string.Empty)}{value:0.##}";
            }

            var list = new List<TooltipProperty>
            {
                new(_TCurrentScore.ToString(), $"{explainedNumber.ResultNumber:0.##}", 0, false, TooltipProperty.TooltipPropertyFlags.Title)
            };

            foreach (var (name, number) in explainedNumber.GetLines())
                list.Add(new TooltipProperty(name, PlusPrefixed(number), 0));

            list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
            list.Add(new TooltipProperty(_TRequiredScore.ToString(), $"{AllianceScoringModel.Instance.ScoreThreshold:0.##}", 0, false,
                TooltipProperty.TooltipPropertyFlags.RundownResult));

            return new BasicTooltipViewModel(() => list);
        }

        [DataSourceMethod]
        [UsedImplicitly]
        public void ExecuteExecutiveAction()
        {
            if (_isAlliance)
            {
                BreakAllianceAction.Apply((Kingdom) ViewModel!.Faction1, (Kingdom) ViewModel!.Faction2);
            }
            else
            {
                DiplomacyCostCalculator.DetermineCostForDeclaringWar((Kingdom) ViewModel!.Faction1, true).ApplyCost();
                DeclareWarAction.Apply(ViewModel!.Faction1, ViewModel!.Faction2);
            }

            OnRefresh();
        }

        [DataSourceMethod]
        [UsedImplicitly]
        public void FormAlliance()
        {
            DeclareAllianceAction.Apply((Kingdom) ViewModel!.Faction1, (Kingdom) ViewModel!.Faction2, true);
            OnRefresh();
        }

        [DataSourceMethod]
        [UsedImplicitly]
        public void ProposeNonAggressionPact()
        {
            FormNonAggressionPactAction.Apply((Kingdom) ViewModel!.Faction1, (Kingdom) ViewModel!.Faction2, true);
            OnRefresh();
        }
    }
}