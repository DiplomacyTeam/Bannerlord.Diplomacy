using Diplomacy.Actions;

using JetBrains.Annotations;

using System;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    /// <summary>
    /// Picker for handing a conquered fief back to the kingdom it was taken from. Deliberately
    /// exposes the same data source names as <see cref="GrantFiefVM"/> so that it can reuse the
    /// GrantFief movie unchanged.
    /// </summary>
    internal sealed class ReturnFiefVM : TaleWorlds.Library.ViewModel
    {
        private static readonly TextObject _TReturnFief = new("{=Zx4nKpWr}Return Fief");
        private static readonly TextObject _TReturnFiefToKingdom = new("{=Ld8vQmTc}Return Fief to {KINGDOM_NAME}");
        private static readonly TextObject _TRelationshipGainWithReceiver = new("{=RxawrCjg}Relationship Gain with Receiver");
        private static readonly TextObject _TFiefReturned = new("{=Pw5jHnBq}Fief Returned");
        private static readonly TextObject _TFiefWasReturned = new("{=Rt9kVdXm}{SETTLEMENT_NAME} was returned to {CLAN_NAME}.");

        private readonly Action _onComplete;
        private readonly Kingdom _targetKingdom;
        private MBBindingList<GrantFiefItemVM> _settlements;

        [DataSourceProperty]
        public MBBindingList<GrantFiefItemVM> Settlements { get => _settlements; set => SetField(ref _settlements, value, nameof(Settlements)); }

        [DataSourceProperty]
        public GrantFiefSortControllerVM SortController { get; }

        // Not a [DataSourceProperty] for the same reason as in GrantFiefVM: the selected item is
        // already in Settlements, this only keeps the reference.
        public GrantFiefItemVM SelectedSettlementItem { get; private set; }

        [DataSourceProperty]
        public string NameText { get; }

        [DataSourceProperty]
        public string TypeText { get; }

        [DataSourceProperty]
        public string ProsperityText { get; }

        [DataSourceProperty]
        public string DefendersText { get; }

        [DataSourceProperty]
        public string GrantFiefCaption { get; }

        [DataSourceProperty]
        public string GrantFiefActionName { get; }

        [DataSourceProperty]
        public string CancelText { get; }

        [DataSourceProperty]
        public string RelationText { get; }

        [DataSourceProperty]
        public HintViewModel RelationHint { get; }

        public ReturnFiefVM(Kingdom targetKingdom, Action onComplete)
        {
            _onComplete = onComplete;
            _targetKingdom = targetKingdom;
            _settlements = new MBBindingList<GrantFiefItemVM>();

            foreach (var settlement in ReturnFiefAction.GetReturnableFiefs(targetKingdom))
                _settlements.Add(new GrantFiefItemVM(settlement, ReturnFiefAction.PreviewRelationChange(settlement, targetKingdom), OnSelect));

            SelectedSettlementItem = _settlements.First();
            SelectedSettlementItem.IsSelected = true;

            SortController = new GrantFiefSortControllerVM(ref _settlements);
            _TReturnFiefToKingdom.SetTextVariable("KINGDOM_NAME", targetKingdom.Name);
            GrantFiefCaption = _TReturnFiefToKingdom.ToString();
            GrantFiefActionName = _TReturnFief.ToString();
            CancelText = GameTexts.FindText("str_cancel").ToString();
            NameText = GameTexts.FindText("str_scoreboard_header", "name").ToString();
            TypeText = GameTexts.FindText("str_sort_by_type_label").ToString();
            ProsperityText = GameTexts.FindText("str_prosperity_abbr").ToString();
            DefendersText = GameTexts.FindText("str_sort_by_defenders_label").ToString();
            RelationText = new TextObject("{=bCOCjOQM}Relat.").ToString();
            RelationHint = Compat.HintViewModel.Create(_TRelationshipGainWithReceiver);
            RefreshValues();
        }

        public void OnSelect(GrantFiefItemVM returnFiefItem)
        {
            SelectedSettlementItem.IsSelected = false;
            SelectedSettlementItem = returnFiefItem;
            SelectedSettlementItem.IsSelected = true;
        }

        // Named for the movie's binding rather than for what it does.
        [UsedImplicitly]
        public void OnGrantFief()
        {
            var settlement = SelectedSettlementItem.Settlement;
            ReturnFiefAction.Apply(settlement, _targetKingdom);

            _TFiefWasReturned.SetTextVariable("SETTLEMENT_NAME", settlement.Name);
            _TFiefWasReturned.SetTextVariable("CLAN_NAME", settlement.OwnerClan?.Name ?? _targetKingdom.Name);

            InformationManager.ShowInquiry(new InquiryData(_TFiefReturned.ToString(),
                _TFiefWasReturned.ToString(),
                true,
                false,
                GameTexts.FindText("str_ok").ToString(),
                null,
                null,
                null));
            _onComplete.Invoke();
        }

        [UsedImplicitly]
        public void OnCancel()
        {
            _onComplete.Invoke();
        }
    }
}
