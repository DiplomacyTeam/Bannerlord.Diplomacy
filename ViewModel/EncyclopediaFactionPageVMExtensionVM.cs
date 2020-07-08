using DiplomacyFixes.DiplomaticAction;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.EncyclopediaItems;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.ViewModel
{
    class EncyclopediaFactionPageVMExtensionVM : EncyclopediaFactionPageVM
    {
        private string _alliesText;
        private IFaction _faction;
        private MBBindingList<EncyclopediaFactionVM> _allies;
        private string _nonAggressionPactsText;
        private MBBindingList<EncyclopediaFactionVM> _nonAggressionPacts;

        public EncyclopediaFactionPageVMExtensionVM(EncyclopediaPageArgs args) : base(args)
        {
        }

        public override void RefreshValues()
        {
            this._faction = (base.Obj as IFaction);
            this.Allies = new MBBindingList<EncyclopediaFactionVM>();
            this.AlliesText = new TextObject("{=KqfNSsBE}Allies", null).ToString();
            this.NonAggressionPacts = new MBBindingList<EncyclopediaFactionVM>();
            this.NonAggressionPactsText = new TextObject(StringConstants.NonAggressionPacts, null).ToString();
            base.RefreshValues();
        }

        public override void Refresh()
        {
            base.IsLoadingOver = false;
            this.Allies.Clear();

            EncyclopediaPage clanPages = Campaign.Current.EncyclopediaManager.GetPageOf(typeof(Clan));
            foreach (IFaction faction in Enumerable.ThenBy<IFaction, string>(Enumerable.OrderBy<IFaction, bool>(Campaign.Current.Factions, (IFaction x) => !x.IsKingdomFaction), (IFaction f) => f.Name.ToString()))
            {
                if (clanPages.IsValidEncyclopediaItem(faction) && faction != this._faction && FactionManager.IsAlliedWithFaction(this._faction, faction))
                {
                    this.Allies.Add(new EncyclopediaFactionVM(faction));
                }
            }

            if (this._faction.IsKingdomFaction)
            {
                foreach (IFaction faction in Enumerable.ThenBy<IFaction, string>(Enumerable.OrderBy<IFaction, bool>(Campaign.Current.Factions, (IFaction x) => !x.IsKingdomFaction), (IFaction f) => f.Name.ToString()))
                {
                    if (clanPages.IsValidEncyclopediaItem(faction) && faction != this._faction && faction.IsKingdomFaction && DiplomaticAgreementManager.Instance.HasNonAggressionPact(this._faction as Kingdom, faction as Kingdom, out _))
                    {
                        this.NonAggressionPacts.Add(new EncyclopediaFactionVM(faction));
                    }
                }
            }

            base.Refresh();
        }

        [DataSourceProperty]
        public string AlliesText
        {
            get
            {
                return this._alliesText;
            }
            set
            {
                if (value != this._alliesText)
                {
                    this._alliesText = value;
                    base.OnPropertyChanged("AlliesText");
                }
            }
        }

        [DataSourceProperty]
        public string NonAggressionPactsText
        {
            get
            {
                return this._nonAggressionPactsText;
            }
            set
            {
                if (value != this._nonAggressionPactsText)
                {
                    this._nonAggressionPactsText = value;
                    base.OnPropertyChanged("NonAggressionPactsText");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFactionVM> Allies
        {
            get
            {
                return this._allies;
            }
            set
            {
                if (value != this._allies)
                {
                    this._allies = value;
                    base.OnPropertyChanged("Allies");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFactionVM> NonAggressionPacts
        {
            get
            {
                return this._nonAggressionPacts;
            }
            set
            {
                if (value != this._nonAggressionPacts)
                {
                    this._nonAggressionPacts = value;
                    base.OnPropertyChanged("NonAggressionPacts");
                }
            }
        }
    }
}
