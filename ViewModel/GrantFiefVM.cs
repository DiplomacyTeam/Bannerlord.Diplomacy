using DiplomacyFixes.GrantFief;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.ViewModel
{
    internal class GrantFiefVM : TaleWorlds.Library.ViewModel
    {
        private Action _onComplete;
        private Hero _targetHero;
        private MBBindingList<GrantFiefItemVM> _settlements;

        public GrantFiefVM(Hero hero, Action onComplete)
        {
            this._onComplete = onComplete;
            this._targetHero = hero;
            this.Settlements = new MBBindingList<GrantFiefItemVM>();
            foreach (Town settlement in Clan.PlayerClan.Fortifications)
            {
                _settlements.Add(new GrantFiefItemVM(settlement.Owner.Settlement, this.OnSelect));
            }
            this.SelectedSettlementItem = this.Settlements.FirstOrDefault();
            this.SelectedSettlementItem.IsSelected = true;
            this.SortController = new GrantFiefSortControllerVM(ref _settlements);
            this.GrantFiefActionName = new TextObject("{=LpoyhORp}Grant Fief").ToString();
            this.CancelText = GameTexts.FindText("str_cancel", null).ToString();
            this.NameText = GameTexts.FindText("str_scoreboard_header", "name").ToString();
            this.TypeText = GameTexts.FindText("str_sort_by_type_label", null).ToString();
            this.ProsperityText = GameTexts.FindText("str_prosperity_abbr", null).ToString();
            this.DefendersText = GameTexts.FindText("str_sort_by_defenders_label", null).ToString();
            this.RefreshValues();
        }

        public void OnSelect(GrantFiefItemVM grantFiefItem)
        {
            SelectedSettlementItem.IsSelected = false;
            SelectedSettlementItem = grantFiefItem;
            SelectedSettlementItem.IsSelected = true;
        }

        public void OnGrantFief()
        {
            GrantFiefAction.Apply(this.SelectedSettlementItem.Settlement, this._targetHero.Clan);
            TextObject text = new TextObject("{=cXbgaPSm}{SETTLEMENT_NAME} was granted to {CLAN_NAME}.");
            text.SetTextVariable("SETTLEMENT_NAME", this.SelectedSettlementItem.Settlement.Name);
            text.SetTextVariable("CLAN_NAME", this._targetHero.Clan.Name);

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
            get { return this._settlements; }

            set
            {
                if (value != this._settlements)
                {
                    this._settlements = value;
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
    }
}