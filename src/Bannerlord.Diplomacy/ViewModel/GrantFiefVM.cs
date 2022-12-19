using Diplomacy.Actions;
using Diplomacy.Extensions;

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
    internal sealed class GrantFiefVM : TaleWorlds.Library.ViewModel
    {
        private static readonly TextObject _TGrantFief = new("{=LpoyhORp}Grant Fief");
        private static readonly TextObject _TGrantFiefToClan = new("{=h3b1S5Wq}Grant Fief to {CLAN_NAME}");
        private static readonly TextObject _TRelationshipGainWithReceiver = new("{=RxawrCjg}Relationship Gain with Receiver");
        private static readonly TextObject _TFiefGranted = new("{=jznJfkfU}Fief Granted");
        private static readonly TextObject _TFiefWasGrantedToClan = new("{=cXbgaPSm}{SETTLEMENT_NAME} was granted to {CLAN_NAME}.");

        private readonly Action _onComplete;
        private readonly Hero _targetHero;
        private MBBindingList<GrantFiefItemVM> _settlements;

        [DataSourceProperty]
        public MBBindingList<GrantFiefItemVM> Settlements { get => _settlements; set => SetField(ref _settlements, value, nameof(Settlements)); }

        [DataSourceProperty]
        public GrantFiefSortControllerVM SortController { get; }

        // Doesn't need a [DataSourceProperty] attribute because the selected item is already in the Settlements list. This is just to keep the reference of the selected item.
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

        public GrantFiefVM(Hero hero, Action onComplete)
        {
            _onComplete = onComplete;
            _targetHero = hero;
            _settlements = new MBBindingList<GrantFiefItemVM>();

            foreach (var settlement in Clan.PlayerClan.GetPermanentFiefs())
                _settlements.Add(new GrantFiefItemVM(settlement.Owner.Settlement, _targetHero, OnSelect));

            SelectedSettlementItem = _settlements.First();
            SelectedSettlementItem.IsSelected = true;

            SortController = new GrantFiefSortControllerVM(ref _settlements);
            _TGrantFiefToClan.SetTextVariable("CLAN_NAME", _targetHero.Clan.Name);
            GrantFiefCaption = _TGrantFiefToClan.ToString();
            GrantFiefActionName = _TGrantFief.ToString();
            CancelText = GameTexts.FindText("str_cancel").ToString();
            NameText = GameTexts.FindText("str_scoreboard_header", "name").ToString();
            TypeText = GameTexts.FindText("str_sort_by_type_label").ToString();
            ProsperityText = GameTexts.FindText("str_prosperity_abbr").ToString();
            DefendersText = GameTexts.FindText("str_sort_by_defenders_label").ToString();
            RelationText = new TextObject("{=bCOCjOQM}Relat.").ToString();
            RelationHint = Compat.HintViewModel.Create(_TRelationshipGainWithReceiver);
            RefreshValues();
        }

        public void OnSelect(GrantFiefItemVM grantFiefItem)
        {
            SelectedSettlementItem.IsSelected = false;
            SelectedSettlementItem = grantFiefItem;
            SelectedSettlementItem.IsSelected = true;
        }

        [UsedImplicitly]
        public void OnGrantFief()
        {
            GrantFiefAction.Apply(SelectedSettlementItem.Settlement, _targetHero.Clan);
            _TFiefWasGrantedToClan.SetTextVariable("SETTLEMENT_NAME", SelectedSettlementItem.Settlement.Name);
            _TFiefWasGrantedToClan.SetTextVariable("CLAN_NAME", _targetHero.Clan.Name);

            InformationManager.ShowInquiry(new InquiryData(_TFiefGranted.ToString(),
                _TFiefWasGrantedToClan.ToString(),
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