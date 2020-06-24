using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace DiplomacyFixes.ViewModel
{
    public class DiplomacyFactionRelationshipVM : TaleWorlds.Library.ViewModel
    {
        private IFaction _faction;
        private ImageIdentifierVM _imageIdentifier;
        private string _nameText;

        public DiplomacyFactionRelationshipVM(IFaction faction)
        {
            this.Faction = faction;
            this.ImageIdentifier = new ImageIdentifierVM(BannerCode.CreateFrom(faction.Banner), true);
            this.NameText = this.Faction.Name.ToString();
        }

        private void ExecuteLink()
        {
            Campaign.Current.EncyclopediaManager.GoToLink(this.Faction.EncyclopediaLink);
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
                return this._nameText;
            }
            set
            {
                if (value != this._nameText)
                {
                    this._nameText = value;
                    base.OnPropertyChanged("NameText");
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM ImageIdentifier
        {
            get
            {
                return this._imageIdentifier;
            }
            set
            {
                if (value != this._imageIdentifier)
                {
                    this._imageIdentifier = value;
                    base.OnPropertyChanged("Banner");
                }
            }
        }

        public IFaction Faction { get => _faction; set => _faction = value; }
    }
}
