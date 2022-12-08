﻿using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using Diplomacy.Costs;
using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.ViewModel;

using JetBrains.Annotations;

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
        private readonly Kingdom _faction1;
        private readonly Kingdom _faction2;

        private HintViewModel? _diplomaticActionHint;
        private int _goldCost;
        private bool _isOptionAvailable;

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

        [DataSourceProperty][UsedImplicitly]
        public bool IsGoldCostVisible { get; } = true;

        [DataSourceProperty]
        public bool IsOptionAvailable { get => _isOptionAvailable; set => SetField(ref _isOptionAvailable, value, nameof(IsOptionAvailable)); }

        [DataSourceProperty]
        public HintViewModel? DiplomaticActionHint { get => _diplomaticActionHint; set => SetField(ref _diplomaticActionHint, value, nameof(DiplomaticActionHint)); }

        public KingdomWarItemVMMixin(KingdomWarItemVM vm) : base(vm)
        {
            _faction1 = (Kingdom) ViewModel!.Faction1;
            _faction2 = (Kingdom) ViewModel!.Faction2;
            var costForMakingPeace = DiplomacyCostCalculator.DetermineCostForMakingPeace(_faction1, _faction2, true);
            InfluenceCost = (int) costForMakingPeace.InfluenceCost.Value;
            GoldCost = (int) costForMakingPeace.GoldCost.Value;
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
                    (int) WarExhaustionManager.Instance.GetWarExhaustion(_faction1, _faction2),
                    (int) WarExhaustionManager.Instance.GetWarExhaustion(_faction2, _faction1),
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
            KingdomPeaceAction.ApplyPeace(_faction1, _faction2, true);
        }

        private void UpdateActionAvailability()
        {
            IsOptionAvailable = MakePeaceConditions.Instance.CanApplyExceptions(ViewModel!).IsEmpty();
            var makePeaceException = MakePeaceConditions.Instance.CanApplyExceptions(ViewModel!).FirstOrDefault();
            DiplomaticActionHint = makePeaceException is not null ? Compat.HintViewModel.Create(makePeaceException) : new HintViewModel();
        }
    }
}