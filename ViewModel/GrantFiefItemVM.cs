using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace DiplomacyFixes.ViewModel
{
    class GrantFiefItemVM : TaleWorlds.Library.ViewModel
    {
        public Settlement Settlement { get; private set; }
        private bool _isSelected;
        private readonly Action<GrantFiefItemVM> _onSelect;

        public GrantFiefItemVM(Settlement settlement, Action<GrantFiefItemVM> onSelect)
        {
            this.Settlement = settlement;
            this.Name = settlement.Name.ToString();
            SettlementComponent component = settlement.GetComponent<SettlementComponent>();
            this.SettlementImagePath = ((component == null) ? "placeholder" : (component.BackgroundMeshName + "_t"));
            Town component2 = settlement.GetComponent<Town>();
            if (component2 != null)
            {
                this.Prosperity = MBMath.Round(component2.Prosperity);
                this.IconPath = component2.BackgroundMeshName;
            }
            else if (settlement.IsCastle)
            {
                this.Prosperity = MBMath.Round(settlement.Prosperity);
                this.IconPath = "";
            }
            this.Garrison = this.Settlement.Town.GarrisonParty?.Party.NumberOfAllMembers ?? 0;
            this._onSelect = onSelect;
        }

        public void ExecuteLink()
        {
            Campaign.Current.EncyclopediaManager.GoToLink(this.Settlement.EncyclopediaLink);
        }

        public void OnSelect()
        {
            this._onSelect(this);
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (value != this._isSelected)
                {
                    this._isSelected = value;
                    base.OnPropertyChanged("IsSelected");
                }
            }
        }

        [DataSourceProperty]
        public string IconPath { get; }

        [DataSourceProperty]
        public int Garrison { get; }

        [DataSourceProperty]
        public string Name { get; }

        [DataSourceProperty]
        public int Prosperity { get; }

        [DataSourceProperty]
        public string SettlementImagePath { get; }
    }
}
