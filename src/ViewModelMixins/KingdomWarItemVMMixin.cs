using System.Linq;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.ViewModel;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModelMixins
{
    [ViewModelMixin("UpdateDiplomacyProperties")]
    internal sealed class KingdomWarItemVMMixin : BaseViewModelMixin<KingdomWarItemVM>
    {
        public KingdomWarItemVMMixin(KingdomWarItemVM vm) : base(vm)
        {
            _faction1 = (Kingdom)ViewModel!.Faction1;
            _faction2 = (Kingdom)ViewModel!.Faction2;
            var costForMakingPeace = DiplomacyCostCalculator.DetermineCostForMakingPeace(_faction1, _faction2, true);
            InfluenceCost = (int)costForMakingPeace.InfluenceCost.Value;
            GoldCost = (int)costForMakingPeace.GoldCost.Value;
            ActionName = GameTexts.FindText("str_kingdom_propose_peace_action", null).ToString();
            AllianceText = _TAlliances.ToString();
            WarsText = _TWars.ToString();
            PactsText = _TNonAggressionPacts.ToString();
            IsWarItem = true;
            OnRefresh();
        }

        [DataSourceProperty]
        public bool IsWarItem { get; }

        private static readonly TextObject _TAlliances = new("{=zpNalMeA}Alliances");
        private static readonly TextObject _TWars = new("{=y5tXjbLK}Wars");
        private static readonly TextObject _TNonAggressionPacts = new("{=noWHMN1W}Non-Aggression Pacts");
        private static readonly TextObject _TWarExhaustion = new("{=XmVTQ0bH}War Exhaustion");

        public override void OnRefresh()
        {
            if (DiplomacyProperties is null)
                DiplomacyProperties = new DiplomacyPropertiesVM(ViewModel!.Faction1, ViewModel!.Faction2);

            DiplomacyProperties.UpdateDiplomacyProperties();

            UpdateActionAvailability();

            if (Settings.Instance!.EnableWarExhaustion)
            {
                ViewModel!.Stats.Insert(1, new KingdomWarComparableStatVM(
                    (int)WarExhaustionManager.Instance.GetWarExhaustion(_faction1, _faction2),
                    (int)WarExhaustionManager.Instance.GetWarExhaustion(_faction2, _faction1),
                    _TWarExhaustion, 
                    Color.FromUint(this.ViewModel!.Faction1.Color).ToString(),
                    Color.FromUint(this.ViewModel!.Faction2.Color).ToString(), 
                    100, 
                    null));
            }
        }

        [DataSourceMethod]
        public void ExecuteExecutiveAction() => KingdomPeaceAction.ApplyPeace(_faction1, _faction2, forcePlayerCharacterCosts: true);

        private void UpdateActionAvailability()
        {
            IsOptionAvailable = MakePeaceConditions.Instance.CanApplyExceptions(ViewModel!, true).IsEmpty();
            var makePeaceException = MakePeaceConditions.Instance.CanApplyExceptions(ViewModel!, true).FirstOrDefault();
            ActionHint = makePeaceException is not null ? Compat.HintViewModel.Create(makePeaceException) : new HintViewModel();
        }

        [DataSourceProperty]
        public DiplomacyPropertiesVM? DiplomacyProperties { get; private set; }

        [DataSourceProperty]
        public string ActionName { get; init; }

        [DataSourceProperty]
        public string AllianceText { get; }

        [DataSourceProperty]
        public string WarsText { get; }

        [DataSourceProperty]
        public string PactsText { get; }

        [DataSourceProperty]
        public int InfluenceCost { get; }

        [DataSourceProperty]
        public int GoldCost
        {
            get => _goldCost;
            set
            {
                if (value != _goldCost)
                {
                    _goldCost = value;
                    ViewModel!.OnPropertyChanged(nameof(GoldCost));
                }
            }
        }

        [DataSourceProperty]
        public bool IsGoldCostVisible { get; } = true;

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

        private HintViewModel? _actionHint;
        private bool _isOptionAvailable;
        private int _goldCost;
        private readonly Kingdom _faction1;
        private readonly Kingdom _faction2;
    }
}