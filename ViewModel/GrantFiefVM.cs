using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.ViewModel
{
    internal class GrantFiefVM : TaleWorlds.Library.ViewModel
    {
        private Action _onComplete;

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
            this.RefreshValues();
        }

        public override void RefreshValues()
        {
            this.NameText = GameTexts.FindText("str_scoreboard_header", "name").ToString();
            this.TypeText = GameTexts.FindText("str_sort_by_type_label", null).ToString();
            this.ProsperityText = GameTexts.FindText("str_prosperity_abbr", null).ToString();
            this.DefendersText = GameTexts.FindText("str_sort_by_defenders_label", null).ToString();
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
            _onComplete.Invoke();
        }

        public void OnCancel()
        {
            _onComplete.Invoke();
        }

        private Hero _targetHero;

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
        public GrantFiefSortControllerVM SortController
        {
            get { return this._sortController; }
            set
            {
                if (value != this._sortController)
                {
                    this._sortController = value;
                    base.OnPropertyChanged("SortController");
                }
            }
        }

        private MBBindingList<GrantFiefItemVM> _settlements;
        private GrantFiefSortControllerVM _sortController;
        private string _nameText;
        private string _prosperityText;
        private string _defendersText;
        private string _typeText;
        private string _grantFiefActionName;
        private string _cancelText;

        public GrantFiefItemVM SelectedSettlementItem { get; private set; }

        [DataSourceProperty]
        public string NameText
        {
            get { return this._nameText; }
            private set
            {
                if (value != this._nameText)
                {
                    this._nameText = value;
                }
                base.OnPropertyChanged("NameText");
            }
        }
        [DataSourceProperty]
        public string TypeText
        {
            get { return this._typeText; }
            private set
            {
                if (value != this._typeText)
                {
                    this._typeText = value;
                }
                base.OnPropertyChanged("TypeText");
            }
        }
        [DataSourceProperty]
        public string ProsperityText
        {
            get { return this._prosperityText; }
            private set
            {
                if (value != this._prosperityText)
                {
                    this._prosperityText = value;
                }
                base.OnPropertyChanged("ProsperityText");
            }
        }
        [DataSourceProperty]
        public string DefendersText
        {
            get { return this._defendersText; }
            private set
            {
                if (value != this._defendersText)
                {
                    this._defendersText = value;
                }
                base.OnPropertyChanged("DefendersText");
            }
        }

        [DataSourceProperty]
        public string GrantFiefActionName
        {
            get { return this._grantFiefActionName; }

            set
            {
                if (value != this._grantFiefActionName)
                {
                    this._grantFiefActionName = value;
                    base.OnPropertyChanged("GrantFiefActionName");
                }
            }
        }

        [DataSourceProperty]
        public string CancelText
        {
            get { return this._cancelText; }

            set
            {
                if (value != this._cancelText)
                {
                    this._cancelText = value;
                    base.OnPropertyChanged("CancelText");
                }
            }
        }
    }
}