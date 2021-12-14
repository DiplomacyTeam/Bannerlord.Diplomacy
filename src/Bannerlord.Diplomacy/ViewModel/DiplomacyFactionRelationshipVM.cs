using JetBrains.Annotations;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace Diplomacy.ViewModel
{
    public sealed class DiplomacyFactionRelationshipVM : TaleWorlds.Library.ViewModel
    {
        public IFaction Faction { get; init; }

        public DiplomacyFactionRelationshipVM(IFaction faction, HintViewModel? hint = null)
        {
            Faction = faction;
            _imageIdentifier = new ImageIdentifierVM(BannerCode.CreateFrom(faction.Banner), true);
            _nameText = Faction.Name.ToString();
            _hint = hint ?? new HintViewModel();
        }

        [UsedImplicitly]
        private void ExecuteLink() => Campaign.Current.EncyclopediaManager.GoToLink(Faction.EncyclopediaLink);

        public override bool Equals(object obj) => obj is DiplomacyFactionRelationshipVM vm && Equals(vm);

        public bool Equals(DiplomacyFactionRelationshipVM vm) => EqualityComparer<IFaction>.Default.Equals(Faction, vm.Faction);

        public override int GetHashCode() => -301155118 + EqualityComparer<IFaction>.Default.GetHashCode(Faction);

        [DataSourceProperty]
        public string NameText
        {
            get => _nameText;
            set
            {
                if (value != _nameText)
                {
                    _nameText = value;
                    OnPropertyChanged(nameof(NameText));
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM ImageIdentifier
        {
            get => _imageIdentifier;
            set
            {
                if (value != _imageIdentifier)
                {
                    _imageIdentifier = value;
                    // FIXME: Property named below was "Banner" -- interpreted as bug, but check functionality switching between ImageIdentifiers
                    OnPropertyChanged(nameof(ImageIdentifier));
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel Hint
        {
            get => _hint;
            set
            {
                if (value != _hint)
                {
                    _hint = value;
                    OnPropertyChanged(nameof(Hint));
                }
            }
        }

        private ImageIdentifierVM _imageIdentifier;
        private string _nameText;
        private HintViewModel _hint;
    }
}