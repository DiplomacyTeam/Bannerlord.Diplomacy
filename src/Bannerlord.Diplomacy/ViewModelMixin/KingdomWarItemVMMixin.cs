using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using Diplomacy.Costs;
using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.Helpers;
using Diplomacy.ViewModel;
using Diplomacy.WarExhaustion;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModelMixin
{
    [ViewModelMixin("UpdateDiplomacyProperties")]
    internal sealed class KingdomWarItemVMMixin : BaseViewModelMixin<KingdomWarItemVM>
    {
        private static readonly TextObject _TAlliances = new("{=zpNalMeA}Alliances");
        private static readonly TextObject _TWars = new("{=y5tXjbLK}Wars");
        private static readonly TextObject _TNonAggressionPacts = new("{=noWHMN1W}Non-Aggression Pacts");
        private static readonly TextObject _TWarExhaustion = new("{=XmVTQ0bH}War Exhaustion");
        private const string _TDiplomaticActionHelpText = "{=Q8ItogIP}Propose directly to {ENEMY_LEADER}{WAR_REPARATIONS_AND_TRIBUTE}{FIEFS_TO_RETURN}";
        private const string _TPayments = "{=BC7823G2}{?REPARATIONS_PAY}, paying {REPARATIONS} war reparations{?TRIBUTE_PAY} and {TRIBUTE} tribute/day.{?}{?TRIBUTE_GET}, but receiving {TRIBUTE} tribute/day.{?}{\\?}{\\?}" +
            "{?}{?REPARATIONS_GET}, receiving {REPARATIONS} war reparations{?TRIBUTE_PAY}, but paying {TRIBUTE} tribute/day.{?}{?TRIBUTE_GET} and {TRIBUTE} tribute/day.{?}{\\?}{\\?}" +
            "{?}{?TRIBUTE_PAY}, paying {TRIBUTE} tribute/day.{?}{?TRIBUTE_GET}, receiving {TRIBUTE} tribute/day.{?}.{\\?}{\\?}{\\?}{\\?}";
        private const string _TFiefs = "{=9CvcPwJA}{?FIEFS_ANY} You may have to return some of the captured fiefs back!{?}{\\?}";

        private readonly Kingdom _faction1;
        private readonly Kingdom _faction2;
        private string? _diplomaticActionHelpText;
        private HintViewModel? _diplomaticActionHint;
        private int _goldCost;
        private bool _isOptionAvailable;
        private readonly List<KingdomWalletCost> _reparations;

        [DataSourceProperty]
        public DiplomacyPropertiesVM? DiplomacyProperties { get; private set; }

        [DataSourceProperty]
        public string ActionName { get; }

        [DataSourceProperty]
        [UsedImplicitly]
        public bool IsAllianceVisible => false;

        [DataSourceProperty]
        [UsedImplicitly]
        public bool IsNonAggressionPactVisible => false;

        [DataSourceProperty]
        public string AllianceText { get; }

        [DataSourceProperty]
        public string WarsText { get; }

        [DataSourceProperty]
        public string PactsText { get; }

        [DataSourceProperty]
        public int InfluenceCost { get; }

        [DataSourceProperty]
        public int GoldCost { get => _goldCost; set => SetField(ref _goldCost, value, nameof(GoldCost)); }

        [DataSourceProperty]
        [UsedImplicitly]
        public bool IsGoldCostVisible { get; } = true;

        [DataSourceProperty]
        public bool IsOptionAvailable { get => _isOptionAvailable; set => SetField(ref _isOptionAvailable, value, nameof(IsOptionAvailable)); }

        [DataSourceProperty]
        public HintViewModel? DiplomaticActionHint { get => _diplomaticActionHint; set => SetField(ref _diplomaticActionHint, value, nameof(DiplomaticActionHint)); }

        [DataSourceProperty]
        public string? DiplomaticActionHelpText { get => _diplomaticActionHelpText; set => SetField(ref _diplomaticActionHelpText, value, nameof(DiplomaticActionHelpText)); }

        public KingdomWarItemVMMixin(KingdomWarItemVM vm) : base(vm)
        {
            _faction1 = (Kingdom) ViewModel!.Faction1;
            _faction2 = (Kingdom) ViewModel!.Faction2;
            var costForMakingPeace = DiplomacyCostCalculator.DetermineCostForMakingPeace(_faction1, _faction2, true);
            InfluenceCost = (int) costForMakingPeace.InfluenceCost.Value;
            GoldCost = (int) costForMakingPeace.GoldCost.Value;
            _reparations = costForMakingPeace.KingdomWalletCosts;
            ActionName = GameTexts.FindText("str_kingdom_propose_peace_action").ToString();
            AllianceText = _TAlliances.ToString();
            WarsText = _TWars.ToString();
            PactsText = _TNonAggressionPacts.ToString();
            OnRefresh();
        }

        public override void OnRefresh()
        {
            DiplomacyProperties ??= new DiplomacyPropertiesVM(_faction1, _faction2);

            DiplomacyProperties.UpdateDiplomacyProperties();

            UpdateActionAvailability();

            if (Settings.Instance!.EnableWarExhaustion)
            {
                if (ViewModel!.Stats[1].Name.Equals(_TWarExhaustion.ToString()))
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

        [DataSourceMethod]
        [UsedImplicitly]
        public void ExecuteExecutiveAction()
        {
            KingdomPeaceAction.ApplyPeace(_faction1, _faction2, forcePlayerCharacterCosts: true, skipPlayerPrompts: true);
        }

        private void UpdateActionAvailability()
        {
            var listOfExceptions = MakePeaceConditions.Instance.CanApplyExceptions(ViewModel!);
            IsOptionAvailable = listOfExceptions.IsEmpty();
            var makePeaceException = listOfExceptions.FirstOrDefault();
            if (IsOptionAvailable)
            {
                var tributeValue = TributeHelper.GetDailyTribute(_faction1, _faction2);
                DiplomaticActionHelpText = new TextObject(_TDiplomaticActionHelpText, new()
                {
                    ["ENEMY_LEADER"] = _faction2.Leader.Name,
                    ["WAR_REPARATIONS_AND_TRIBUTE"] = new TextObject(_TPayments, new()
                    {
                        ["REPARATIONS_PAY"] = _reparations.Any(r => r.PayingKingdom == _faction1 && r.Value > 0) ? 1 : 0,
                        ["REPARATIONS_GET"] = _reparations.Any(r => r.PayingKingdom == _faction2 && r.Value > 0) ? 1 : 0,
                        ["REPARATIONS"] = GetReparationsText(_reparations.Sum(r => r.Value)),
                        ["TRIBUTE_PAY"] = tributeValue > 0 ? 1 : 0,
                        ["TRIBUTE_GET"] = tributeValue < 0 ? 1 : 0,
                        ["TRIBUTE"] = Math.Abs(tributeValue),
                    }),
                    ["FIEFS_TO_RETURN"] = new TextObject(_TFiefs, new() { ["FIEFS_ANY"] = KingdomPeaceAction.GetFiefsSuitableToBeReturned(_faction1, _faction2).Any() ? 1 : 0 }),
                }).ToString();
            }
            else
                DiplomaticActionHelpText = string.Empty;
            DiplomaticActionHint = makePeaceException is not null ? Compat.HintViewModel.Create(makePeaceException) : new HintViewModel();
        }

        private static string GetReparationsText(float value)
        {
            return value switch
            {
                >= 1000000 => new TextObject("{=6xcW68Tw}{VALUE}M", new() { ["VALUE"] = (value / 1000000).ToString("F2") }).ToString(),
                >= 1000 => new TextObject("{=oD4fZ2tY}{VALUE}K", new() { ["VALUE"] = (value / 1000).ToString("F1") }).ToString(),
                _ => new TextObject().ToString()
            };
        }
    }
}