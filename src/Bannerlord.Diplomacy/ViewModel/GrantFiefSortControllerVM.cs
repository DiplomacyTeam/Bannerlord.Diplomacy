using System.Collections.Generic;

using TaleWorlds.Library;

namespace Diplomacy.ViewModel
{
    class GrantFiefSortControllerVM : TaleWorlds.Library.ViewModel
    {
        public GrantFiefSortControllerVM(ref MBBindingList<GrantFiefItemVM> listToControl)
        {
            _listToControl = listToControl;
            _typeComparer = new ItemTypeComparer();
            _prosperityComparer = new ItemProsperityComparer();
            _defendersComparer = new ItemDefendersComparer();
            _relationComparer = new ItemRelationComparer();
            _nameComparer = new ItemNameComparer();
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

        private void SetAllStates(SortState state)
        {
            TypeState = (int) state;
            NameState = (int) state;
            ProsperityState = (int) state;
            DefendersState = (int) state;
            RelationState = (int) state;
            IsTypeSelected = false;
            IsNameSelected = false;
            IsProsperitySelected = false;
            IsDefendersSelected = false;
            IsRelationSelected = false;
        }

        [DataSourceProperty]
        public int TypeState { get => _typeState; set => SetField(ref _typeState, value, nameof(TypeState)); }

        [DataSourceProperty]
        public int NameState { get => _nameState; set => SetField(ref _nameState, value, nameof(NameState)); }

        [DataSourceProperty]
        public int ProsperityState { get => _prosperityState; set => SetField(ref _prosperityState, value, nameof(ProsperityState)); }

        [DataSourceProperty]
        public int RelationState { get => _relationState; set => SetField(ref _relationState, value, nameof(RelationState)); }

        [DataSourceProperty]
        public int DefendersState { get => _defendersState; set => SetField(ref _defendersState, value, nameof(DefendersState)); }

        [DataSourceProperty]
        public bool IsTypeSelected { get => _isTypeSelected; set => SetField(ref _isTypeSelected, value, nameof(IsTypeSelected)); }

        [DataSourceProperty]
        public bool IsNameSelected { get => _isNameSelected; set => SetField(ref _isNameSelected, value, nameof(IsNameSelected)); }

        [DataSourceProperty]
        public bool IsDefendersSelected { get => _isDefendersSelected; set => SetField(ref _isDefendersSelected, value, nameof(IsDefendersSelected)); }

        [DataSourceProperty]
        public bool IsProsperitySelected { get => _isProsperitySelected; set => SetField(ref _isProsperitySelected, value, nameof(IsProsperitySelected)); }

        [DataSourceProperty]
        public bool IsRelationSelected { get => _isRelationSelected; set => SetField(ref _isRelationSelected, value, nameof(IsRelationSelected)); }

        private readonly MBBindingList<GrantFiefItemVM> _listToControl;

        private readonly ItemTypeComparer _typeComparer;

        private readonly ItemProsperityComparer _prosperityComparer;

        private readonly ItemRelationComparer _relationComparer;

        private readonly ItemDefendersComparer _defendersComparer;

        private readonly ItemNameComparer _nameComparer;

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

        private class ItemNameComparer : ItemComparerBase
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

        private class ItemClanComparer : ItemComparerBase
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

        private class ItemTypeComparer : ItemComparerBase
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

        private class ItemProsperityComparer : ItemComparerBase
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

        private class ItemRelationComparer : ItemComparerBase
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

        private class ItemFoodComparer : ItemComparerBase
        {
            public override int Compare(GrantFiefItemVM x, GrantFiefItemVM y)
            {
                var num = y.Settlement.Town?.FoodStocks ?? 0f;
                var value = x.Settlement.Town?.FoodStocks ?? 0f;
                if (_isAcending)
                {
                    return num.CompareTo(value) * -1;
                }
                return num.CompareTo(value);
            }
        }

        private class ItemGarrisonComparer : ItemComparerBase
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

        private class ItemDefendersComparer : ItemComparerBase
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