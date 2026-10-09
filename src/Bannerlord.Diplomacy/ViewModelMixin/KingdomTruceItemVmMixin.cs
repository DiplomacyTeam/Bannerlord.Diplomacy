using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using Diplomacy.Actions;
using Diplomacy.Costs;
using Diplomacy.DiplomaticAction;
using Diplomacy.DiplomaticAction.NonAggressionPact;
using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.Extensions;
using Diplomacy.GauntletInterfaces;
using Diplomacy.Helpers;
using Diplomacy.ViewModel;
using Diplomacy.WarExhaustion;

using JetBrains.Annotations;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;

namespace Diplomacy.ViewModelMixin
{
    [ViewModelMixin("UpdateDiplomacyProperties")]
    [UsedImplicitly]
    internal sealed class KingdomTruceItemVMMixin : BaseViewModelMixin<KingdomTruceItemVM>
    {
        private static readonly TextObject _TFormPact = new("{=9pY0NQrk}Form Pact");
        private static readonly TextObject _TWars = new("{=y5tXjbLK}Wars");
        private static readonly TextObject _TAlliances = new("{=zpNalMeA}Alliances");
        private static readonly TextObject _TPacts = new(StringConstants.NonAggressionPacts);

        private static readonly TextObject _TDirectActionExplanation = new("{=tsY4iHnO}Impose your will without calling a council");

        private static readonly TextObject _TNapHelpText = new("{=9zlQNtlX}Form a non-aggression pact lasting {DAYS} days.");
        private static readonly TextObject _TWarExhaustion = new("{=XmVTQ0bH}War Exhaustion");

        private static readonly TextObject _TBreakAlliance = new("{=K4GraLTn}Break Alliance");
        private static readonly TextObject _TDeclareWar = new("{=nkOVblQ4}Declare War");

        private static readonly TextObject _TReturnFief = new("{=Zx4nKpWr}Return Fief");
        private static readonly TextObject _TReturnFiefHelp = new("{=Hn2wKpRv}Hand back land you took from them.");
        private static readonly TextObject _TNothingToReturn = new("{=Cf6qWsNe}You hold no land taken from {KINGDOM}.");

        private static readonly TextObject _TRequiredScore = new("{=XIBUWDlT}Required Score");
        private static readonly TextObject _TCurrentScore = new("{=5r6fsHgm}Current Score");

        private readonly Kingdom _faction1;
        private readonly Kingdom _faction2;
        private readonly ReturnFiefInterface _returnFiefInterface = new();
        private bool _isReturnFiefVisible;
        private bool _isReturnFiefAvailable;
        private HintViewModel? _returnFiefHint;
        private HintViewModel? _directActionHint;
        private string _directActionName = null!;
        private HintViewModel? _allianceHint;
        private BasicTooltipViewModel? _allianceScoreHint;
        private bool _isAllianceVisible;
        private bool _isNonAggressionPactVisible;
        private bool _isNonAggressionPactAvailable;
        private bool _isDirectActionEnabled;
        private bool _isDirectActionVisible;
        private int _nonAggressionPactGoldCost;
        private HintViewModel? _nonAggressionPactHint;
        private int _nonAggressionPactInfluenceCost;
        int _directActionInfluenceCost;
        private BasicTooltipViewModel? _nonAggressionPactScoreHint;
        private string? _directActionExplanationText;

        [DataSourceProperty]
                public bool IsAllianceVisible { get => _isAllianceVisible; set => SetField(ref _isAllianceVisible, value, nameof(IsAllianceVisible)); }

        [DataSourceProperty]
        public bool IsNonAggressionPactVisible { get => _isNonAggressionPactVisible; set => SetField(ref _isNonAggressionPactVisible, value, nameof(IsNonAggressionPactVisible)); }

        [DataSourceProperty]
        public bool IsNonAggressionPactAvailable { get => _isNonAggressionPactAvailable; set => SetField(ref _isNonAggressionPactAvailable, value, nameof(IsNonAggressionPactAvailable)); }

        [DataSourceProperty]
        public int NonAggressionPactInfluenceCost { get => _nonAggressionPactInfluenceCost; set => SetField(ref _nonAggressionPactInfluenceCost, value, nameof(NonAggressionPactInfluenceCost)); }

        [DataSourceProperty]
        public int NonAggressionPactGoldCost { get => _nonAggressionPactGoldCost; set => SetField(ref _nonAggressionPactGoldCost, value, nameof(NonAggressionPactGoldCost)); }

        [DataSourceProperty]
        public string DirectActionName { get => _directActionName; set => SetField(ref _directActionName, value, nameof(DirectActionName)); }

        [DataSourceProperty]
        public string NonAggressionPactActionName { get; }

        [DataSourceProperty]
        public string AllianceText { get; }

        [DataSourceProperty]
        public string WarsText { get; }

        [DataSourceProperty]
        public string PactsText { get; }

        [DataSourceProperty]
        public int DirectActionInfluenceCost { get => _directActionInfluenceCost; set => SetField(ref _directActionInfluenceCost, value, nameof(DirectActionInfluenceCost)); }

        [DataSourceProperty]
        public bool IsDirectActionEnabled { get => _isDirectActionEnabled; set => SetField(ref _isDirectActionEnabled, value, nameof(IsDirectActionEnabled)); }

        [DataSourceProperty]
        public bool IsDirectActionVisible { get => _isDirectActionVisible; set => SetField(ref _isDirectActionVisible, value, nameof(IsDirectActionVisible)); }

        [DataSourceProperty]
        public HintViewModel? DirectActionHint { get => _directActionHint; set => SetField(ref _directActionHint, value, nameof(DirectActionHint)); }

        [DataSourceProperty]
        public HintViewModel? AllianceHint { get => _allianceHint; set => SetField(ref _allianceHint, value, nameof(AllianceHint)); }

        [DataSourceProperty]
        public HintViewModel? NonAggressionPactHint { get => _nonAggressionPactHint; set => SetField(ref _nonAggressionPactHint, value, nameof(NonAggressionPactHint)); }

        [DataSourceProperty]
        public BasicTooltipViewModel? AllianceScoreHint { get => _allianceScoreHint; set => SetField(ref _allianceScoreHint, value, nameof(AllianceScoreHint)); }

        [DataSourceProperty]
        public BasicTooltipViewModel? NonAggressionPactScoreHint { get => _nonAggressionPactScoreHint; set => SetField(ref _nonAggressionPactScoreHint, value, nameof(NonAggressionPactScoreHint)); }

        [DataSourceProperty]
        public string? DirectActionExplanationText { get => _directActionExplanationText; set => SetField(ref _directActionExplanationText, value, nameof(DirectActionExplanationText)); }

        [DataSourceProperty]
        [UsedImplicitly]
        public bool IsGoldCostVisible { get; }

        [DataSourceProperty]
        public string NonAggressionPactHelpText { get; }

        [DataSourceProperty]
        public DiplomacyPropertiesVM? DiplomacyProperties { get; set; }

        [DataSourceProperty]
        public bool IsReturnFiefVisible { get => _isReturnFiefVisible; set => SetField(ref _isReturnFiefVisible, value, nameof(IsReturnFiefVisible)); }

        [DataSourceProperty]
        public bool IsReturnFiefAvailable { get => _isReturnFiefAvailable; set => SetField(ref _isReturnFiefAvailable, value, nameof(IsReturnFiefAvailable)); }

        [DataSourceProperty]
        public HintViewModel? ReturnFiefHint { get => _returnFiefHint; set => SetField(ref _returnFiefHint, value, nameof(ReturnFiefHint)); }

        [DataSourceProperty]
        public string ReturnFiefActionName { get; }

        [DataSourceProperty]
        public string ReturnFiefHelpText { get; }

        public KingdomTruceItemVMMixin(KingdomTruceItemVM vm) : base(vm)
        {
            _faction1 = (Kingdom) ViewModel!.Faction1;
            _faction2 = (Kingdom) ViewModel!.Faction2;
            DirectActionExplanationText = _TDirectActionExplanation.ToString();
            NonAggressionPactActionName = _TFormPact.ToString();
            AllianceText = _TAlliances.ToString();
            WarsText = _TWars.ToString();
            PactsText = _TPacts.ToString();
            NonAggressionPactHelpText = _TNapHelpText.SetTextVariable("DAYS", Settings.Instance!.NonAggressionPactDuration).ToString();
            ReturnFiefActionName = _TReturnFief.ToString();
            ReturnFiefHelpText = _TReturnFiefHelp.ToString();
            OnRefresh();
        }

        public override void OnRefresh()
        {
            DiplomacyProperties ??= new DiplomacyPropertiesVM(_faction1, _faction2);

            DiplomacyProperties.UpdateDiplomacyProperties();
            UpdateActionAvailability();
            UpdateReturnFiefAvailability();

            if (Settings.Instance!.EnableWarExhaustion)
            {
                if (ViewModel!.Stats.Count > 1 && ViewModel!.Stats[1].Name.Equals(_TWarExhaustion.ToString()))
                    ViewModel!.Stats.RemoveAt(1);

                // Stats is cleared and repopulated by UpdateDiplomacyProperties, so it can be empty here
                var insertIndex = ViewModel!.Stats.Count > 0 ? 1 : 0;

                ViewModel!.Stats.Insert(insertIndex, new KingdomWarComparableStatVM(
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
            if (ViewModel!.HasAlliance)
            {
                DirectActionName = _TBreakAlliance.ToString();
                DirectActionHint = new HintViewModel();
                IsDirectActionVisible = true;
                IsDirectActionEnabled = true;
                IsAllianceVisible = false;
                IsNonAggressionPactVisible = false;
                return;
            }

            var declareWarException = DeclareWarConditions.Instance.CanApplyExceptions(ViewModel!).FirstOrDefault();
            var napException = NonAggressionPactConditions.Instance.CanApplyExceptions(ViewModel!).FirstOrDefault();

            DirectActionName = _TDeclareWar.ToString();
            IsDirectActionVisible = true;
            IsDirectActionEnabled = declareWarException is null;
            DirectActionHint = declareWarException is not null ? Compat.HintViewModel.Create(declareWarException) : new HintViewModel();
            DirectActionInfluenceCost = ViewModel!.HasAlliance ? 0 : (int) DiplomacyCostCalculator.DetermineCostForDeclaringWar(_faction1, true).Value;

            IsAllianceVisible = true;

            IsNonAggressionPactVisible = !DiplomaticAgreementManager.HasNonAggressionPact(_faction1, _faction2, out _);
            IsNonAggressionPactAvailable = napException is null;
            NonAggressionPactHint = napException is not null ? Compat.HintViewModel.Create(napException) : new HintViewModel();

            var nonAggressionPactCost = DiplomacyCostCalculator.DetermineCostForFormingNonAggressionPact(_faction1, _faction2, true);
            NonAggressionPactInfluenceCost = (int) nonAggressionPactCost.InfluenceCost.Value;
            NonAggressionPactGoldCost = (int) nonAggressionPactCost.GoldCost.Value;

            var napScore = NonAggressionPactScoringModel.Instance.GetScore(_faction2, _faction1, true);
            NonAggressionPactScoreHint = UpdateDiplomacyTooltip(napScore);
        }

        private void UpdateReturnFiefAvailability()
        {
            IsReturnFiefVisible = Settings.Instance!.EnableFiefReturn
                                  && Clan.PlayerClan.MapFaction?.Leader == Hero.MainHero;

            if (!IsReturnFiefVisible)
                return;

            var takenFromThem = Clan.PlayerClan.GetPermanentFiefs()
                .Select(town => town.Settlement)
                .Where(settlement => FiefProvenanceManager.Instance?.WasConqueredFrom(settlement, _faction2, out _) ?? false)
                .ToList();

            IsReturnFiefAvailable = takenFromThem.Any(settlement => ReturnFiefAction.CanReturnFief(settlement, _faction2, out _));

            if (IsReturnFiefAvailable)
            {
                ReturnFiefHint = new HintViewModel();
                return;
            }

            // Holding land of theirs means the refusal is specific - usually a cooldown - and so
            // worth showing in place of the generic explanation.
            string? reason = null;
            foreach (var settlement in takenFromThem)
            {
                ReturnFiefAction.CanReturnFief(settlement, _faction2, out reason);
                if (reason is not null)
                    break;
            }

            ReturnFiefHint = Compat.HintViewModel.Create(new TextObject(reason ?? _TNothingToReturn.SetTextVariable("KINGDOM", _faction2.Name).ToString()));
        }

        [DataSourceMethod]
        [UsedImplicitly]
        public void ExecuteReturnFief()
        {
            _returnFiefInterface.ShowReturnFiefInterface(ScreenManager.TopScreen, _faction2, OnRefresh);
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

            return new BasicTooltipViewModel(() => list);
        }

        [DataSourceMethod]
        [UsedImplicitly]
        public void ExecuteDirectAction()
        {
            if (ViewModel!.HasAlliance)
            {
                Campaign.Current.GetCampaignBehavior<AllianceCampaignBehavior>().EndAlliance(_faction1, _faction2);
            }
            else
            {
                DiplomacyCostCalculator.DetermineCostForDeclaringWar(_faction1, true).ApplyCost();
                DeclareWarAction.ApplyByKingdomDecision(_faction1, _faction2);
            }

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
