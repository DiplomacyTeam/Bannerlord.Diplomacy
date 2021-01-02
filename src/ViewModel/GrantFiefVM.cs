using Diplomacy.Extensions;
using Diplomacy.GrantFief;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    internal class GrantFiefVM : TaleWorlds.Library.ViewModel
    {
        private Action _onComplete;
        private Hero _targetHero;
        private MBBindingList<GrantFiefItemVM> _settlements;

        public GrantFiefVM(Hero hero, Action onComplete)
        {
            _onComplete = onComplete;
            _targetHero = hero;
            Settlements = new MBBindingList<GrantFiefItemVM>();
            foreach (var settlement in Clan.PlayerClan.GetPermanentFiefs())
            {
                _settlements.Add(new GrantFiefItemVM(settlement.Owner.Settlement, _targetHero, OnSelect));
            }
            SelectedSettlementItem = Settlements.FirstOrDefault();
            SelectedSettlementItem.IsSelected = true;
            SortController = new GrantFiefSortControllerVM(ref _settlements);
            GrantFiefActionName = new TextObject("{=LpoyhORp}Grant Fief").ToString();
            CancelText = GameTexts.FindText("str_cancel", null).ToString();
            NameText = GameTexts.FindText("str_scoreboard_header", "name").ToString();
            TypeText = GameTexts.FindText("str_sort_by_type_label", null).ToString();
            ProsperityText = GameTexts.FindText("str_prosperity_abbr", null).ToString();
            DefendersText = GameTexts.FindText("str_sort_by_defenders_label", null).ToString();
            RelationText = new TextObject("{=bCOCjOQM}Relat.").ToString();
            RelationHint = new HintViewModel(new TextObject("{=RxawrCjg}Relationship Gain with Grantee").ToString());
            RefreshValues();
        }

        public void OnSelect(GrantFiefItemVM grantFiefItem)
        {
            SelectedSettlementItem.IsSelected = false;
            SelectedSettlementItem = grantFiefItem;
            SelectedSettlementItem.IsSelected = true;
        }

        public void OnGrantFief()
        {
            GrantFiefAction.Apply(SelectedSettlementItem.Settlement, _targetHero.Clan);
            var text = new TextObject("{=cXbgaPSm}{SETTLEMENT_NAME} was granted to {CLAN_NAME}.");
            text.SetTextVariable("SETTLEMENT_NAME", SelectedSettlementItem.Settlement.Name);
            text.SetTextVariable("CLAN_NAME", _targetHero.Clan.Name);

            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=jznJfkfU}Fief Granted").ToString(), text.ToString(), true, false, GameTexts.FindText("str_ok", null).ToString(), null, null, null), false);
            _onComplete.Invoke();
        }

        public void OnCancel()
        {
            _onComplete.Invoke();
        }


        [DataSourceProperty]
        public MBBindingList<GrantFiefItemVM> Settlements
        {
            get { return _settlements; }

            set
            {
                if (value != _settlements)
                {
                    _settlements = value;
                    base.OnPropertyChanged("Settlements");
                }
            }
        }

        [DataSourceProperty]
        public GrantFiefSortControllerVM SortController { get; }

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
        public string GrantFiefActionName { get; }

        [DataSourceProperty]
        public string CancelText { get; }

        [DataSourceProperty]
        public string RelationText { get; }

        [DataSourceProperty]
        public HintViewModel RelationHint { get; }
    }
}
