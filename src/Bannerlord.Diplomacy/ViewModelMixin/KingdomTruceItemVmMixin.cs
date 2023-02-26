using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using Diplomacy.Costs;
using Diplomacy.DiplomaticAction;
using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.DiplomaticAction.NonAggressionPact;
using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.Helpers;
using Diplomacy.ViewModel;
using Diplomacy.WarExhaustion;

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
    internal sealed class KingdomTruceItemVMMixin : BaseViewModelMixin<KingdomTruceItemVM>
    {
        private static readonly TextObject _TFormAlliance = new("{=0WPWbx70}Form Alliance");
        private static readonly TextObject _TFormPact = new("{=9pY0NQrk}Form Pact");
        private static readonly TextObject _TWars = new("{=y5tXjbLK}Wars");
        private static readonly TextObject _TAlliances = new("{=zpNalMeA}Alliances");
        private static readonly TextObject _TPacts = new(StringConstants.NonAggressionPacts);

        private static readonly TextObject _TNapHelpText = new("{=9zlQNtlX}Form a non-aggression pact lasting {DAYS} days.");
        private static readonly TextObject _TWarExhaustion = new("{=XmV_TQ0bH}War Exhaustion");

        private static readonly TextObject _TBreakAlliance = new("{=K4GraLTn}Break Alliance");

        private static readonly TextObject _TRequiredScore = new("{=XIBUWDlT}Required Score");
        private static readonly TextObject _TCurrentScore = new("{=5r6fsHgm}Current Score");

        private readonly Kingdom _faction1;
        private readonly Kingdom _faction2;
        private readonly bool _isAlliance;
        private HintViewModel? _diplomaticActionHint;
        private string _actionName = null!;
        private int _allianceGoldCost;
        private HintViewModel? _allianceHint;
        private int _allianceInfluenceCost;
        private BasicTooltipViewModel? _allianceScoreHint;
        private bool _isAllianceVisible;
        private bool _isAllianceAvailable;
        private bool _isNonAggressionPactVisible;
        private bool _isNonAggressionPactAvailable;
        private bool _isOptionAvailable;
        private int _nonAggressionPactGoldCost;
        private HintViewModel? _nonAggressionPactHint;
        private int _nonAggressionPactInfluenceCost;
        private BasicTooltipViewModel? _nonAggressionPactScoreHint;

        [DataSourceProperty]
        public bool IsAllianceVisible { get => _isAllianceVisible; set => SetField(ref _isAllianceVisible, value, nameof(IsAllianceVisible)); }

        [DataSourceProperty]
        public bool IsAllianceAvailable { get => _isAllianceAvailable; set => SetField(ref _isAllianceAvailable, value, nameof(IsAllianceAvailable)); }

        [DataSourceProperty]
        public int AllianceInfluenceCost { get => _allianceInfluenceCost; set => SetField(ref _allianceInfluenceCost, value, nameof(AllianceInfluenceCost)); }

        [DataSourceProperty]
        public int AllianceGoldCost { get => _allianceGoldCost; set => SetField(ref _allianceGoldCost, value, nameof(AllianceGoldCost)); }

        [DataSourceProperty]
        public bool IsNonAggressionPactVisible { get => _isNonAggressionPactVisible; set => SetField(ref _isNonAggressionPactVisible, value, nameof(IsNonAggressionPactVisible)); }

        [DataSourceProperty]
        public bool IsNonAggressionPactAvailable { get => _isNonAggressionPactAvailable; set => SetField(ref _isNonAggressionPactAvailable, value, nameof(IsNonAggressionPactAvailable)); }

        [DataSourceProperty]
        public int NonAggressionPactInfluenceCost { get => _nonAggressionPactInfluenceCost; set => SetField(ref _nonAggressionPactInfluenceCost, value, nameof(NonAggressionPactInfluenceCost)); }

        [DataSourceProperty]
        public int NonAggressionPactGoldCost { get => _nonAggressionPactGoldCost; set => SetField(ref _nonAggressionPactGoldCost, value, nameof(NonAggressionPactGoldCost)); }

        [DataSourceProperty]
        public string ActionName { get => _actionName; set => SetField(ref _actionName, value, nameof(ActionName)); }

        [DataSourceProperty]
        public string NonAggressionPactActionName { get; }

        [DataSourceProperty]
        public string AllianceText { get; }

        [DataSourceProperty]
        public string WarsText { get; }

        [DataSourceProperty]
        public string PactsText { get; }

        [DataSourceProperty]
        public int InfluenceCost { get; }

        [DataSourceProperty]
        public bool IsOptionAvailable { get => _isOptionAvailable; set => SetField(ref _isOptionAvailable, value, nameof(IsOptionAvailable)); }

        [DataSourceProperty]
        public HintViewModel? DiplomaticActionHint { get => _diplomaticActionHint; set => SetField(ref _diplomaticActionHint, value, nameof(DiplomaticActionHint)); }

        [DataSourceProperty]
        public HintViewModel? AllianceHint { get => _allianceHint; set => SetField(ref _allianceHint, value, nameof(AllianceHint)); }

        [DataSourceProperty]
        public HintViewModel? NonAggressionPactHint { get => _nonAggressionPactHint; set => SetField(ref _nonAggressionPactHint, value, nameof(NonAggressionPactHint)); }

        [DataSourceProperty]
        public BasicTooltipViewModel? AllianceScoreHint { get => _allianceScoreHint; set => SetField(ref _allianceScoreHint, value, nameof(AllianceScoreHint)); }

        [DataSourceProperty]
        public BasicTooltipViewModel? NonAggressionPactScoreHint { get => _nonAggressionPactScoreHint; set => SetField(ref _nonAggressionPactScoreHint, value, nameof(NonAggressionPactScoreHint)); }

        [DataSourceProperty]
        public string AllianceActionName { get; }

        [DataSourceProperty]
        [UsedImplicitly]
        public bool IsGoldCostVisible { get; }

        [DataSourceProperty]
        public string NonAggressionPactHelpText { get; }

        [DataSourceProperty]
        public DiplomacyPropertiesVM? DiplomacyProperties { get; set; }

        public KingdomTruceItemVMMixin(KingdomTruceItemVM vm) : base(vm)
        {
            _faction1 = (Kingdom) ViewModel!.Faction1;
            _faction2 = (Kingdom) ViewModel!.Faction2;
            AllianceActionName = _TFormAlliance.ToString();
            NonAggressionPactActionName = _TFormPact.ToString();
            AllianceText = _TAlliances.ToString();
            WarsText = _TWars.ToString();
            PactsText = _TPacts.ToString();
            NonAggressionPactHelpText = _TNapHelpText.SetTextVariable("DAYS", Settings.Instance!.NonAggressionPactDuration).ToString();
            _isAlliance = _faction1.GetStanceWith(_faction2).IsAllied;
            ActionName = _isAlliance ? _TBreakAlliance.ToString() : GameTexts.FindText("str_kingdom_declate_war_action").ToString();
            InfluenceCost = _isAlliance ? 0 : (int) DiplomacyCostCalculator.DetermineCostForDeclaringWar(_faction1, true).Value;
            OnRefresh();
        }

        public override void OnRefresh()
        {
            DiplomacyProperties ??= new DiplomacyPropertiesVM(_faction1, _faction2);

            DiplomacyProperties.UpdateDiplomacyProperties();
            UpdateActionAvailability();

            if (Settings.Instance!.EnableWarExhaustion)
            {
                if (ViewModel!.Stats.Count > 1 && ViewModel!.Stats[1].Name.Equals(_TWarExhaustion.ToString()))
                    ViewModel!.Stats.RemoveAt(1);

                ViewModel!.Stats.Insert(1, new KingdomWarComparableStatVM(
                    (int) WarExhaustionManager.Instance!.GetWarExhaustion(_faction1, _faction2),
                    (int) WarExhaustionManager.Instance!.GetWarExhaustion(_faction2, _faction1),
                    _TWarExhaustion,
                    Color.FromUint(_faction1.Color).ToString(),
                    Color.FromUint(_faction2.Color).ToString(),
                    100));
            }
        }

        private void UpdateActionAvailability()
        {
            if (_isAlliance)
            {
                var breakAllianceException = BreakAllianceConditions.Instance.CanApplyExceptions(ViewModel!).FirstOrDefault();
                DiplomaticActionHint = breakAllianceException is not null ? Compat.HintViewModel.Create(breakAllianceException) : new HintViewModel();
                IsOptionAvailable = breakAllianceException is null;
                IsAllianceVisible = false;
                IsNonAggressionPactVisible = false;
                return;
            }

            IsOptionAvailable = DeclareWarConditions.Instance.CanApplyExceptions(ViewModel!).IsEmpty();

            var allianceException = FormAllianceConditions.Instance.CanApplyExceptions(ViewModel!).FirstOrDefault();
            var declareWarException = DeclareWarConditions.Instance.CanApplyExceptions(ViewModel!).FirstOrDefault();
            var napException = NonAggressionPactConditions.Instance.CanApplyExceptions(ViewModel!).FirstOrDefault();

            IsAllianceVisible = true;
            IsNonAggressionPactVisible = !DiplomaticAgreementManager.HasNonAggressionPact(_faction1, _faction2, out _);
            IsAllianceAvailable = allianceException is null;
            IsNonAggressionPactAvailable = napException is null;

            DiplomaticActionHint = declareWarException is not null ? Compat.HintViewModel.Create(declareWarException) : new HintViewModel();
            AllianceHint = allianceException is not null ? Compat.HintViewModel.Create(allianceException) : new HintViewModel();
            NonAggressionPactHint = napException is not null ? Compat.HintViewModel.Create(napException) : new HintViewModel();

            var allianceCost = DiplomacyCostCalculator.DetermineCostForFormingAlliance(_faction1, _faction2, true);
            AllianceInfluenceCost = (int) allianceCost.InfluenceCost.Value;
            AllianceGoldCost = (int) allianceCost.GoldCost.Value;

            var nonAggressionPactCost = DiplomacyCostCalculator.DetermineCostForFormingNonAggressionPact(_faction1, _faction2, true);
            NonAggressionPactInfluenceCost = (int) nonAggressionPactCost.InfluenceCost.Value;
            NonAggressionPactGoldCost = (int) nonAggressionPactCost.GoldCost.Value;

            var allianceScore = AllianceScoringModel.Instance.GetScore(_faction2, _faction1, true);
            var napScore = NonAggressionPactScoringModel.Instance.GetScore(_faction2, _faction1, true);
            AllianceScoreHint = UpdateDiplomacyTooltip(allianceScore);
            NonAggressionPactScoreHint = UpdateDiplomacyTooltip(napScore);
        }

        private BasicTooltipViewModel UpdateDiplomacyTooltip(ExplainedNumber explainedNumber)
        {
            var list = new List<TooltipProperty>
            {
                new(_TCurrentScore.ToString(), $"{explainedNumber.ResultNumber:0.##}", 0, false, TooltipProperty.TooltipPropertyFlags.Title)
            };

            foreach (var (name, number) in explainedNumber.GetLines())
                list.Add(new TooltipProperty(name, StringHelper.GetPlusPrefixed(number), 0));

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
                BreakAllianceAction.Apply(_faction1, _faction2);
            }
            else
            {
                DiplomacyCostCalculator.DetermineCostForDeclaringWar(_faction1, true).ApplyCost();
#if v100 || v101 || v102 || v103
                DeclareWarAction.Apply(_faction1, _faction2);
#else
                DeclareWarAction.ApplyByKingdomDecision(_faction1, _faction2);
#endif
            }

            OnRefresh();
        }

        [DataSourceMethod]
        [UsedImplicitly]
        public void FormAlliance()
        {
            DeclareAllianceAction.Apply(_faction1, _faction2, true);
            OnRefresh();
        }

        [DataSourceMethod]
        [UsedImplicitly]
        public void ProposeNonAggressionPact()
        {
            FormNonAggressionPactAction.Apply(_faction1, _faction2, true);
            OnRefresh();
        }
    }
}