using Diplomacy.Actions;

using JetBrains.Annotations;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Diplomacy.ViewModel
{
    internal class GrantFiefItemVM : TaleWorlds.Library.ViewModel
    {
        public Settlement Settlement { get; }
        private bool _isSelected;
        private readonly Action<GrantFiefItemVM> _onSelect;

        public GrantFiefItemVM(Settlement settlement, Hero targetHero, Action<GrantFiefItemVM> onSelect)
        {
            Settlement = settlement;
            Name = settlement.Name.ToString();
            var component = settlement.SettlementComponent;
            SettlementImagePath = ((component is null) ? "placeholder" : (component.BackgroundMeshName + "_t"));
            var component2 = settlement.Town;
            if (component2 is not null)
            {
                Prosperity = (int) Math.Round(component2.Prosperity);
                IconPath = component2.BackgroundMeshName;
            }
            else if (settlement.IsCastle)
            {
#if v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115 || v116
                Prosperity = (int) Math.Round(settlement.Prosperity);
#else
                Prosperity = (int) Math.Round(settlement.Town.Prosperity);
#endif
                IconPath = "";
            }
            Garrison = Settlement.Town.GarrisonParty?.Party.NumberOfAllMembers ?? 0;
            _onSelect = onSelect;
            RelationBonus = string.Concat(new[] { GrantFiefAction.PreviewPositiveRelationChange(Settlement, targetHero).ToString(), "+" });
        }

        [UsedImplicitly]
        public void ExecuteLink()
        {
            Campaign.Current.EncyclopediaManager.GoToLink(Settlement.EncyclopediaLink);
        }

        [UsedImplicitly]
        public void OnSelect()
        {
            _onSelect(this);
        }

        [DataSourceProperty]
        public bool IsSelected { get => _isSelected; set => SetField(ref _isSelected, value, nameof(IsSelected)); }

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