using Diplomacy.GrantFief;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Diplomacy.ViewModel
{
    internal class GrantFiefItemVM : TaleWorlds.Library.ViewModel
    {
        public Settlement Settlement { get; private set; }
        private bool _isSelected;
        private readonly Action<GrantFiefItemVM> _onSelect;

        public GrantFiefItemVM(Settlement settlement, Hero targetHero, Action<GrantFiefItemVM> onSelect)
        {
            Settlement = settlement;
            Name = settlement.Name.ToString();
            var component = settlement.GetComponent<SettlementComponent>();
            SettlementImagePath = ((component is null) ? "placeholder" : (component.BackgroundMeshName + "_t"));
            var component2 = settlement.GetComponent<Town>();
            if (component2 is not null)
            {
                Prosperity = MBMath.Round(component2.Prosperity);
                IconPath = component2.BackgroundMeshName;
            }
            else if (settlement.IsCastle)
            {
                Prosperity = MBMath.Round(settlement.Prosperity);
                IconPath = "";
            }
            Garrison = Settlement.Town.GarrisonParty?.Party.NumberOfAllMembers ?? 0;
            _onSelect = onSelect;
            RelationBonus = string.Concat(new string[] { GrantFiefAction.PreviewPositiveRelationChange(Settlement, targetHero).ToString(), "+" });
        }

        public void ExecuteLink()
        {
            Campaign.Current.EncyclopediaManager.GoToLink(Settlement.EncyclopediaLink);
        }

        public void OnSelect()
        {
            _onSelect(this);
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        [DataSourceProperty]
        public string? IconPath { get; }

        [DataSourceProperty]
        public int Garrison { get; }

        [DataSourceProperty]
        public string Name { get; }

        [DataSourceProperty]
        public int Prosperity { get; }

        [DataSourceProperty]
        public string SettlementImagePath { get; }

        [DataSourceProperty]
        public string RelationBonus { get; }
    }
}
