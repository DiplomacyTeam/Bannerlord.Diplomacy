using System.Collections.Generic;
using TaleWorlds.Library;

namespace DiplomacyFixes.ViewModel
{
    class GrantFiefSortControllerVM : TaleWorlds.Library.ViewModel
    {

        public GrantFiefSortControllerVM(ref MBBindingList<GrantFiefItemVM> listToControl)
        {
            this._listToControl = listToControl;
            this._typeComparer = new GrantFiefSortControllerVM.ItemTypeComparer();
            this._prosperityComparer = new GrantFiefSortControllerVM.ItemProsperityComparer();
            this._defendersComparer = new GrantFiefSortControllerVM.ItemDefendersComparer();
            this._nameComparer = new GrantFiefSortControllerVM.ItemNameComparer();
        }

        private void ExecuteSortByType()
        {
            int typeState = this.TypeState;
            this.SetAllStates(GrantFiefSortControllerVM.SortState.Default);
            this.TypeState = (typeState + 1) % 3;
            if (this.TypeState == 0)
            {
                this.TypeState++;
            }
            this._typeComparer.SetSortMode(this.TypeState == 1);
            this._listToControl.Sort(this._typeComparer);
            this.IsTypeSelected = true;
        }

        private void ExecuteSortByName()
        {
            int nameState = this.NameState;
            this.SetAllStates(GrantFiefSortControllerVM.SortState.Default);
            this.NameState = (nameState + 1) % 3;
            if (this.NameState == 0)
            {
                this.NameState++;
            }
            this._nameComparer.SetSortMode(this.NameState == 1);
            this._listToControl.Sort(this._nameComparer);
            this.IsNameSelected = true;
        }

        private void ExecuteSortByProsperity()
        {
            int prosperityState = this.ProsperityState;
            this.SetAllStates(GrantFiefSortControllerVM.SortState.Default);
            this.ProsperityState = (prosperityState + 1) % 3;
            if (this.ProsperityState == 0)
            {
                this.ProsperityState++;
            }
            this._prosperityComparer.SetSortMode(this.ProsperityState == 1);
            this._listToControl.Sort(this._prosperityComparer);
            this.IsProsperitySelected = true;
        }

        private void ExecuteSortByDefenders()
        {
            int defendersState = this.DefendersState;
            this.SetAllStates(GrantFiefSortControllerVM.SortState.Default);
            this.DefendersState = (defendersState + 1) % 3;
            if (this.DefendersState == 0)
            {
                int defendersState2 = this.DefendersState;
                this.DefendersState = defendersState2 + 1;
            }
            this._defendersComparer.SetSortMode(this.DefendersState == 1);
            this._listToControl.Sort(this._defendersComparer);
            this.IsDefendersSelected = true;
        }

        private void SetAllStates(GrantFiefSortControllerVM.SortState state)
        {
            this.TypeState = (int)state;
            this.NameState = (int)state;
            this.ProsperityState = (int)state;
            this.DefendersState = (int)state;
            this.IsTypeSelected = false;
            this.IsNameSelected = false;
            this.IsProsperitySelected = false;
            this.IsDefendersSelected = false;
        }

        [DataSourceProperty]
        public int TypeState
        {
            get
            {
                return this._typeState;
            }
            set
            {
                if (value != this._typeState)
                {
                    this._typeState = value;
                    base.OnPropertyChanged("TypeState");
                }
            }
        }

        [DataSourceProperty]
        public int NameState
        {
            get
            {
                return this._nameState;
            }
            set
            {
                if (value != this._nameState)
                {
                    this._nameState = value;
                    base.OnPropertyChanged("NameState");
                }
            }
        }

        [DataSourceProperty]
        public int ProsperityState
        {
            get
            {
                return this._prosperityState;
            }
            set
            {
                if (value != this._prosperityState)
                {
                    this._prosperityState = value;
                    base.OnPropertyChanged("ProsperityState");
                }
            }
        }

        [DataSourceProperty]
        public int DefendersState
        {
            get
            {
                return this._defendersState;
            }
            set
            {
                if (value != this._defendersState)
                {
                    this._defendersState = value;
                    base.OnPropertyChanged("DefendersState");
                }
            }
        }

        [DataSourceProperty]
        public bool IsTypeSelected
        {
            get
            {
                return this._isTypeSelected;
            }
            set
            {
                if (value != this._isTypeSelected)
                {
                    this._isTypeSelected = value;
                    base.OnPropertyChanged("IsTypeSelected");
                }
            }
        }

        [DataSourceProperty]
        public bool IsNameSelected
        {
            get
            {
                return this._isNameSelected;
            }
            set
            {
                if (value != this._isNameSelected)
                {
                    this._isNameSelected = value;
                    base.OnPropertyChanged("IsNameSelected");
                }
            }
        }

        [DataSourceProperty]
        public bool IsDefendersSelected
        {
            get
            {
                return this._isDefendersSelected;
            }
            set
            {
                if (value != this._isDefendersSelected)
                {
                    this._isDefendersSelected = value;
                    base.OnPropertyChanged("IsDefendersSelected");
                }
            }
        }

        [DataSourceProperty]
        public bool IsProsperitySelected
        {
            get
            {
                return this._isProsperitySelected;
            }
            set
            {
                if (value != this._isProsperitySelected)
                {
                    this._isProsperitySelected = value;
                    base.OnPropertyChanged("IsProsperitySelected");
                }
            }
        }

        private readonly MBBindingList<GrantFiefItemVM> _listToControl;

        private readonly GrantFiefSortControllerVM.ItemTypeComparer _typeComparer;

        private readonly GrantFiefSortControllerVM.ItemProsperityComparer _prosperityComparer;

        private readonly GrantFiefSortControllerVM.ItemDefendersComparer _defendersComparer;

        private readonly GrantFiefSortControllerVM.ItemNameComparer _nameComparer;

        private int _typeState;

        private int _nameState;

        private int _prosperityState;

        private int _defendersState;

        private bool _isTypeSelected;

        private bool _isNameSelected;

        private bool _isProsperitySelected;

        private bool _isDefendersSelected;

        private enum SortState
        {
            Default,
            Ascending,
            Descending
        }

        private abstract class ItemComparerBase : IComparer<GrantFiefItemVM>
        {
            public void SetSortMode(bool isAcending)
            {
                this._isAcending = isAcending;
            }

            public abstract int Compare(GrantFiefItemVM x, GrantFiefItemVM y);

            protected bool _isAcending;
        }

        private class ItemNameComparer : GrantFiefSortControllerVM.ItemComparerBase
        {
            public override int Compare(GrantFiefItemVM x, GrantFiefItemVM y)
            {
                if (this._isAcending)
                {
                    return y.Settlement.Name.ToString().CompareTo(x.Settlement.Name.ToString()) * -1;
                }
                return y.Settlement.Name.ToString().CompareTo(x.Settlement.Name.ToString());
            }
        }

        private class ItemClanComparer : GrantFiefSortControllerVM.ItemComparerBase
        {
            public override int Compare(GrantFiefItemVM x, GrantFiefItemVM y)
            {
                if (this._isAcending)
                {
                    return y.Settlement.OwnerClan.Name.ToString().CompareTo(x.Settlement.OwnerClan.Name.ToString()) * -1;
                }
                return y.Settlement.OwnerClan.Name.ToString().CompareTo(x.Settlement.OwnerClan.Name.ToString());
            }
        }

        private class ItemTypeComparer : GrantFiefSortControllerVM.ItemComparerBase
        {
            public override int Compare(GrantFiefItemVM x, GrantFiefItemVM y)
            {
                if (this._isAcending)
                {
                    return x.Settlement.IsCastle.CompareTo(y.Settlement.IsCastle);
                }
                return x.Settlement.IsCastle.CompareTo(y.Settlement.IsCastle) * -1;
            }
        }

        private class ItemProsperityComparer : GrantFiefSortControllerVM.ItemComparerBase
        {
            public override int Compare(GrantFiefItemVM x, GrantFiefItemVM y)
            {
                if (this._isAcending)
                {
                    return y.Prosperity.CompareTo(x.Prosperity) * -1;
                }
                return y.Prosperity.CompareTo(x.Prosperity);
            }
        }

        private class ItemFoodComparer : GrantFiefSortControllerVM.ItemComparerBase
        {
            public override int Compare(GrantFiefItemVM x, GrantFiefItemVM y)
            {
                float num = (y.Settlement.Town != null) ? y.Settlement.Town.FoodStocks : 0f;
                float value = (x.Settlement.Town != null) ? x.Settlement.Town.FoodStocks : 0f;
                if (this._isAcending)
                {
                    return num.CompareTo(value) * -1;
                }
                return num.CompareTo(value);
            }
        }

        private class ItemGarrisonComparer : GrantFiefSortControllerVM.ItemComparerBase
        {
            public override int Compare(GrantFiefItemVM x, GrantFiefItemVM y)
            {
                if (this._isAcending)
                {
                    return y.Garrison.CompareTo(x.Garrison) * -1;
                }
                return y.Garrison.CompareTo(x.Garrison);
            }
        }

        private class ItemDefendersComparer : GrantFiefSortControllerVM.ItemComparerBase
        {
            public override int Compare(GrantFiefItemVM x, GrantFiefItemVM y)
            {
                if (this._isAcending)
                {
                    return y.Garrison.CompareTo(x.Garrison) * -1;
                }
                return y.Garrison.CompareTo(x.Garrison);
            }
        }
    }
}
