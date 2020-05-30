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
            this._faction = faction;
            this.ImageIdentifier = new ImageIdentifierVM(BannerCode.CreateFrom(faction.Banner), true);
            this.NameText = this._faction.Name.ToString();
        }

        private void ExecuteLink()
        {
            Campaign.Current.EncyclopediaManager.GoToLink(this._faction.EncyclopediaLink);
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
    }
}
