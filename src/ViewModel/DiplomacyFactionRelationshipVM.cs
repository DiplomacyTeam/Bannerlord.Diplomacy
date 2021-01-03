using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace Diplomacy.ViewModel
{
    public class DiplomacyFactionRelationshipVM : TaleWorlds.Library.ViewModel
    {
        private ImageIdentifierVM _imageIdentifier;
        private string _nameText;
        private HintViewModel _hint;

        public IFaction Faction { get; }

        public DiplomacyFactionRelationshipVM(IFaction faction) : this(faction, new HintViewModel()) { }

        public DiplomacyFactionRelationshipVM(IFaction faction, HintViewModel hint)
        {
            Faction = faction;
            ImageIdentifier = new ImageIdentifierVM(BannerCode.CreateFrom(faction.Banner), true);
            NameText = Faction.Name.ToString();
            Hint = hint;
        }

        private void ExecuteLink()
        {
            Campaign.Current.EncyclopediaManager.GoToLink(Faction.EncyclopediaLink);
        }

        public override bool Equals(object obj)
        {
            return obj is DiplomacyFactionRelationshipVM vM &&
                   EqualityComparer<IFaction>.Default.Equals(Faction, vM.Faction);
        }

        public override int GetHashCode()
        {
            return -301155118 + EqualityComparer<IFaction>.Default.GetHashCode(Faction);
        }

        [DataSourceProperty]
        public string NameText
        {
            get
            {
                return _nameText;
            }
            set
            {
                if (value != _nameText)
                {
                    _nameText = value;
                    OnPropertyChanged("NameText");
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM ImageIdentifier
        {
            get
            {
                return _imageIdentifier;
            }
            set
            {
                if (value != _imageIdentifier)
                {
                    _imageIdentifier = value;
                    OnPropertyChanged("Banner");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel Hint
        {
            get
            {
                return _hint;
            }
            set
            {
                if (value != _hint)
                {
                    _hint = value;
                    OnPropertyChanged("Hint");
                }
            }
        }
    }
}
