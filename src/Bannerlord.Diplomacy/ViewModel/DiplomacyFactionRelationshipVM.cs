using JetBrains.Annotations;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
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
        public string NameText { get => _nameText; set => SetField(ref _nameText, value, nameof(NameText)); }

        [DataSourceProperty]
        // TODO: FIXME: Property named below was "Banner" -- interpreted as bug, but check functionality switching between ImageIdentifiers
        public ImageIdentifierVM ImageIdentifier { get => _imageIdentifier; set => SetField(ref _imageIdentifier, value, nameof(ImageIdentifier)); }

        [DataSourceProperty]
        public HintViewModel Hint { get => _hint; set => SetField(ref _hint, value, nameof(Hint)); }

        private ImageIdentifierVM _imageIdentifier;
        private string _nameText;
        private HintViewModel _hint;
    }
}