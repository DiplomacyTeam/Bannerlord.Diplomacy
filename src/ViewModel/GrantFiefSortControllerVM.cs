using System.Collections.Generic;
using TaleWorlds.Library;

namespace Diplomacy.ViewModel
{
    class GrantFiefSortControllerVM : TaleWorlds.Library.ViewModel
    {

        public GrantFiefSortControllerVM(ref MBBindingList<GrantFiefItemVM> listToControl)
        {
            _listToControl = listToControl;
            _typeComparer = new GrantFiefSortControllerVM.ItemTypeComparer();
            _prosperityComparer = new GrantFiefSortControllerVM.ItemProsperityComparer();
            _defendersComparer = new GrantFiefSortControllerVM.ItemDefendersComparer();
            _relationComparer = new GrantFiefSortControllerVM.ItemRelationComparer();
            _nameComparer = new GrantFiefSortControllerVM.ItemNameComparer();
        }

        private void ExecuteSortByType()
        {
            var typeState = TypeState;
            SetAllStates(SortState.Default);
            TypeState = (typeState + 1) % 3;
            if (TypeState == 0)
            {
                TypeState++;
            }
            _typeComparer.SetSortMode(TypeState == 1);
            _listToControl.Sort(_typeComparer);
            IsTypeSelected = true;
        }

        private void ExecuteSortByName()
        {
            var nameState = NameState;
            SetAllStates(SortState.Default);
            NameState = (nameState + 1) % 3;
            if (NameState == 0)
            {
                NameState++;
            }
            _nameComparer.SetSortMode(NameState == 1);
            _listToControl.Sort(_nameComparer);
            IsNameSelected = true;
        }

        private void ExecuteSortByProsperity()
        {
            var prosperityState = ProsperityState;
            SetAllStates(SortState.Default);
            ProsperityState = (prosperityState + 1) % 3;
            if (ProsperityState == 0)
            {
                ProsperityState++;
            }
            _prosperityComparer.SetSortMode(ProsperityState == 1);
            _listToControl.Sort(_prosperityComparer);
            IsProsperitySelected = true;
        }

        private void ExecuteSortByRelation()
        {
            var relationState = RelationState;
            SetAllStates(SortState.Default);
            RelationState = (relationState + 1) % 3;
            if (RelationState == 0)
            {
                RelationState++;
            }
            _relationComparer.SetSortMode(RelationState == 1);
            _listToControl.Sort(_relationComparer);
            IsRelationSelected = true;
        }

        private void ExecuteSortByDefenders()
        {
            var defendersState = DefendersState;
            SetAllStates(SortState.Default);
            DefendersState = (defendersState + 1) % 3;
            if (DefendersState == 0)
            {
                var defendersState2 = DefendersState;
                DefendersState = defendersState2 + 1;
            }
            _defendersComparer.SetSortMode(DefendersState == 1);
            _listToControl.Sort(_defendersComparer);
            IsDefendersSelected = true;
        }

        private void SetAllStates(GrantFiefSortControllerVM.SortState state)
        {
            TypeState = (int)state;
            NameState = (int)state;
            ProsperityState = (int)state;
            DefendersState = (int)state;
            RelationState = (int)state;
            IsTypeSelected = false;
            IsNameSelected = false;
            IsProsperitySelected = false;
            IsDefendersSelected = false;
            IsRelationSelected = false;
        }

        [DataSourceProperty]
        public int TypeState
        {
            get
            {
                return _typeState;
            }
            set
            {
                if (value != _typeState)
                {
                    _typeState = value;
                    OnPropertyChanged("TypeState");
                }
            }
        }

        [DataSourceProperty]
        public int NameState
        {
            get
            {
                return _nameState;
            }
            set
            {
                if (value != _nameState)
                {
                    _nameState = value;
                    OnPropertyChanged("NameState");
                }
            }
        }

        [DataSourceProperty]
        public int ProsperityState
        {
            get
            {
                return _prosperityState;
            }
            set
            {
                if (value != _prosperityState)
                {
                    _prosperityState = value;
                    OnPropertyChanged("ProsperityState");
                }
            }
        }

        [DataSourceProperty]
        public int RelationState
        {
            get
            {
                return _relationState;
            }
            set
            {
                if (value != _relationState)
                {
                    _relationState = value;
                    OnPropertyChanged("RelationState");
                }
            }
        }

        [DataSourceProperty]
        public int DefendersState
        {
            get
            {
                return _defendersState;
            }
            set
            {
                if (value != _defendersState)
                {
                    _defendersState = value;
                    OnPropertyChanged("DefendersState");
                }
            }
        }

        [DataSourceProperty]
        public bool IsTypeSelected
        {
            get
            {
                return _isTypeSelected;
            }
            set
            {
                if (value != _isTypeSelected)
                {
                    _isTypeSelected = value;
                    OnPropertyChanged("IsTypeSelected");
                }
            }
        }

        [DataSourceProperty]
        public bool IsNameSelected
        {
            get
            {
                return _isNameSelected;
            }
            set
            {
                if (value != _isNameSelected)
                {
                    _isNameSelected = value;
                    OnPropertyChanged("IsNameSelected");
                }
            }
        }

        [DataSourceProperty]
        public bool IsDefendersSelected
        {
            get
            {
                return _isDefendersSelected;
            }
            set
            {
                if (value != _isDefendersSelected)
                {
                    _isDefendersSelected = value;
                    OnPropertyChanged("IsDefendersSelected");
                }
            }
        }

        [DataSourceProperty]
        public bool IsProsperitySelected
        {
            get
            {
                return _isProsperitySelected;
            }
            set
            {
                if (value != _isProsperitySelected)
                {
                    _isProsperitySelected = value;
                    OnPropertyChanged("IsProsperitySelected");
                }
            }
        }

        [DataSourceProperty]
        public bool IsRelationSelected
        {
            get
            {
                return _isRelationSelected;
            }
            set
            {
                if (value != _isRelationSelected)
                {
                    _isRelationSelected = value;
                    OnPropertyChanged("IsRelationSelected");
                }
            }
        }

        private readonly MBBindingList<GrantFiefItemVM> _listToControl;

        private readonly GrantFiefSortControllerVM.ItemTypeComparer _typeComparer;

        private readonly GrantFiefSortControllerVM.ItemProsperityComparer _prosperityComparer;

        private readonly GrantFiefSortControllerVM.ItemRelationComparer _relationComparer;

        private readonly GrantFiefSortControllerVM.ItemDefendersComparer _defendersComparer;

        private readonly GrantFiefSortControllerVM.ItemNameComparer _nameComparer;

        private int _typeState;

        private int _nameState;

        private int _prosperityState;

        private int _relationState;

        private int _defendersState;

        private bool _isTypeSelected;

        private bool _isNameSelected;

        private bool _isProsperitySelected;

        private bool _isRelationSelected;

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
                _isAcending = isAcending;
            }

            public abstract int Compare(GrantFiefItemVM x, GrantFiefItemVM y);

            protected bool _isAcending;
        }

        private class ItemNameComparer : GrantFiefSortControllerVM.ItemComparerBase
        {
            public override int Compare(GrantFiefItemVM x, GrantFiefItemVM y)
            {
                if (_isAcending)
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
                if (_isAcending)
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
                if (_isAcending)
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
                if (_isAcending)
                {
                    return y.Prosperity.CompareTo(x.Prosperity) * -1;
                }
                return y.Prosperity.CompareTo(x.Prosperity);
            }
        }

        private class ItemRelationComparer : GrantFiefSortControllerVM.ItemComparerBase
        {
            public override int Compare(GrantFiefItemVM x, GrantFiefItemVM y)
            {
                if (_isAcending)
                {
                    return y.RelationBonus.CompareTo(x.RelationBonus) * -1;
                }
                return y.RelationBonus.CompareTo(x.RelationBonus);
            }
        }

        private class ItemFoodComparer : GrantFiefSortControllerVM.ItemComparerBase
        {
            public override int Compare(GrantFiefItemVM x, GrantFiefItemVM y)
            {
                var num = (y.Settlement.Town is not null) ? y.Settlement.Town.FoodStocks : 0f;
                var value = (x.Settlement.Town is not null) ? x.Settlement.Town.FoodStocks : 0f;
                if (_isAcending)
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
                if (_isAcending)
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
                if (_isAcending)
                {
                    return y.Garrison.CompareTo(x.Garrison) * -1;
                }
                return y.Garrison.CompareTo(x.Garrison);
            }
        }
    }
}
