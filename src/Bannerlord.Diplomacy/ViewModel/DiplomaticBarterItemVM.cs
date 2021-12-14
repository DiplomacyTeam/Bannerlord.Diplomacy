using System;
using System.ComponentModel;
using System.Linq;
using Diplomacy.DiplomaticAction.Barter.Barterables;
using JetBrains.Annotations;
using TaleWorlds.Library;

namespace Diplomacy.ViewModel
{
    internal class DiplomaticBarterItemVM : TaleWorlds.Library.ViewModel
    {
        private readonly Action<DiplomaticBarterItemVM> _addItem;
        private readonly AbstractDiplomaticBarterable _item;
        private readonly Action<DiplomaticBarterItemVM> _removeItem;
        private readonly Action<DiplomaticBarterItemVM> _showPropertyChange;
        private bool _canSelect;
        private int _duration;
        private bool _isOption;
        private int _newDuration;
        private int _newAmount;
        private float _score;
        private bool _showProperties;
        private int _amount;

        public AbstractDiplomaticBarterable Item { get; }

        [DataSourceProperty] public string Name { get; set; }

        [DataSourceProperty] public bool HasAmount { get; }
        [DataSourceProperty] public bool HasDuration { get; }

        [DataSourceProperty]
        public int Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                if (Item is IAmountBarterItem valueBarterItem)
                {
                    valueBarterItem.Amount = value;
                }
                OnPropertyChanged(nameof(Amount));
            }
        }

        [DataSourceProperty]
        public int Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                if (Item is IDurationBarterItem durationBarterItem)
                {
                    durationBarterItem.Duration = value;
                }
                OnPropertyChanged(nameof(Duration));
            }
        }

        [DataSourceProperty] public bool IsValid { get; set; }
        [DataSourceProperty] public float InfluenceCost { get; }

        [DataSourceProperty]
        public int NewAmount
        {
            get => _newAmount;
            set
            {
                _newAmount = value;
                OnPropertyChanged(nameof(NewAmount));
            }
        }

        [DataSourceProperty]
        public int NewDuration
        {
            get => _newDuration;
            set
            {
                _newDuration = value;
                OnPropertyChanged(nameof(NewDuration));
            }
        }


        [DataSourceProperty] public int MaxDuration { get; }

        [DataSourceProperty] public int MinDuration { get; }

        [DataSourceProperty] public int MaxAmount { get; }

        [DataSourceProperty] public int MinAmount { get; }

        [DataSourceProperty] public bool HasEditableProperties { get; }

        [DataSourceProperty]
        public bool ShowProperties
        {
            get => _showProperties;
            set
            {
                _showProperties = value;
                OnPropertyChanged(nameof(ShowProperties));
            }
        }

        [DataSourceProperty]
        public bool IsOption
        {
            get => _isOption;
            set
            {
                _isOption = value;
                OnPropertyChanged(nameof(IsOption));
                ShowProperties = !IsOption && HasEditableProperties;
            }
        }

        [DataSourceProperty]
        public float Score
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged(nameof(Score));
            }
        }

        public bool CanSelect
        {
            get => _canSelect;
            set
            {
                _canSelect = value;
                OnPropertyChanged(nameof(CanSelect));
            }
        }

        public DiplomaticBarterItemVM(AbstractDiplomaticBarterable item,
            bool isValid,
            Action<DiplomaticBarterItemVM> addItem,
            Action<DiplomaticBarterItemVM> removeItem,
            Action<DiplomaticBarterItemVM> showPropertyChange)
        {
            Name = item.Name.ToString();
            Item = item;
            _item = item;
            _addItem = addItem;
            _removeItem = removeItem;
            _showPropertyChange = showPropertyChange;

            IsValid = isValid;
            InfluenceCost = item.InfluenceCost.Value;

            // ReSharper disable once AssignmentInConditionalExpression
            if (item is IAmountBarterItem valueBarterItem)
            {
                MinAmount = valueBarterItem.MinAmount;
                MaxAmount = valueBarterItem.MaxAmount;
                HasAmount = true;
                Amount = valueBarterItem.Amount;
                NewAmount = Amount;
            }

            if (item is IDurationBarterItem durationBarterItem)
            {
                MinDuration = durationBarterItem.MinDuration;
                MaxDuration = durationBarterItem.MaxDuration;
                HasDuration = true;
                Duration = durationBarterItem.Duration;
                NewDuration = Duration;
            }

            HasEditableProperties = !(Item is IBreakableAgreement agreement && agreement.IsBreakAgreement) && HasDuration || HasAmount;
            IsOption = false;

            PropertyChanged += HandlePropertyChanged;
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(CanSelect)) RefreshCanSelect();

            var propertiesToRefreshScore = new[] { nameof(Amount), nameof(Duration), nameof(IsOption) };

            if (propertiesToRefreshScore.Contains(e.PropertyName)) RefreshScore();
        }

        private void RefreshScore()
        {
            Score = GetScore();
        }

        private void RefreshCanSelect()
        {
            CanSelect = IsOption || HasEditableProperties;
        }

        public float GetScore()
        {
            return _item.GetNetScore();
        }

        [UsedImplicitly]
        public void OpenChangeValue()
        {
            _showPropertyChange(this);
        }

        [UsedImplicitly]
        public void AddItemToProposal()
        {
            AddItemToProposalInternal(false);
        }

        public void AddItemToProposalInternal(bool hasSetValue)
        {
            if (!hasSetValue && HasEditableProperties)
                _showPropertyChange(this);
            else
                _addItem(this);
        }

        [UsedImplicitly]
        public void RemoveItemFromProposal()
        {
            _removeItem(this);
        }
    }
}