using Diplomacy.DiplomaticAction.WarPeace;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    internal sealed class KingdomWarItemVMExtensionVM : KingdomWarItemVM
    {
        private static readonly TextObject _TAlliances = new("{=zpNalMeA}Alliances");
        private static readonly TextObject _TWars = new("{=y5tXjbLK}Wars");
        private static readonly TextObject _TNonAggressionPacts = new("{=noWHMN1W}Non-Aggression Pacts");
        private static readonly TextObject _TWarExhaustion = new("{=XmVTQ0bH}War Exhaustion");

        public KingdomWarItemVMExtensionVM(StanceLink stanceLink, Action<KingdomWarItemVM> onSelect, Action<KingdomWarItemVM> onAction) : base(stanceLink, onSelect, onAction)
        {
            var costForMakingPeace = DiplomacyCostCalculator.DetermineCostForMakingPeace((Kingdom)Faction1, (Kingdom)Faction2, true);
            InfluenceCost = (int)costForMakingPeace.InfluenceCost.Value;
            GoldCost = (int)costForMakingPeace.GoldCost.Value;
            ActionName = GameTexts.FindText("str_kingdom_propose_peace_action", null).ToString();
            AllianceText = _TAlliances.ToString();
            WarsText = _TWars.ToString();
            PactsText = _TNonAggressionPacts.ToString();
            UpdateDiplomacyProperties();
        }

        protected override void UpdateDiplomacyProperties()
        {
            if (DiplomacyProperties is null)
                DiplomacyProperties = new DiplomacyPropertiesVM(Faction1, Faction2);

            DiplomacyProperties.UpdateDiplomacyProperties();

            base.UpdateDiplomacyProperties();
            UpdateActionAvailability();

            if (Settings.Instance!.EnableWarExhaustion)
            {
                Stats.Insert(1, new KingdomWarComparableStatVM(
                    (int)WarExhaustionManager.Instance.GetWarExhaustion((Kingdom)Faction1, (Kingdom)Faction2),
                    (int)WarExhaustionManager.Instance.GetWarExhaustion((Kingdom)Faction2, (Kingdom)Faction1),
                    _TWarExhaustion, _faction1Color, _faction2Color, 100, null));
            }
        }
        private void ExecuteExecutiveAction() => KingdomPeaceAction.ApplyPeace((Kingdom)Faction1, (Kingdom)Faction2, forcePlayerCharacterCosts: true);

        private void UpdateActionAvailability()
        {
            IsOptionAvailable = MakePeaceConditions.Instance.CanApplyExceptions(this, true).IsEmpty();
            var makePeaceException = MakePeaceConditions.Instance.CanApplyExceptions(this, true).FirstOrDefault();
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
                    OnPropertyChanged(nameof(GoldCost));
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
                    OnPropertyChanged(nameof(IsOptionAvailable));
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
                    OnPropertyChanged(nameof(ActionHint));
                }
            }
        }

        private HintViewModel? _actionHint;
        private bool _isOptionAvailable;
        private int _goldCost;
    }
}
