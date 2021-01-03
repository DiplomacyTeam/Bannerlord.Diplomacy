using Diplomacy.DiplomaticAction;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.EncyclopediaItems;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
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
            _faction = (Obj as IFaction);
            Allies = new MBBindingList<EncyclopediaFactionVM>();
            AlliesText = new TextObject("{=KqfNSsBE}Allies", null).ToString();
            NonAggressionPacts = new MBBindingList<EncyclopediaFactionVM>();
            NonAggressionPactsText = new TextObject(StringConstants.NonAggressionPacts, null).ToString();
            base.RefreshValues();
        }

        public override void Refresh()
        {
            IsLoadingOver = false;
            Allies.Clear();

            var clanPages = Campaign.Current.EncyclopediaManager.GetPageOf(typeof(Clan));
            foreach (var faction in Enumerable.ThenBy<IFaction, string>(Enumerable.OrderBy<IFaction, bool>(Campaign.Current.Factions, (IFaction x) => !x.IsKingdomFaction), (IFaction f) => f.Name.ToString()))
            {
                if (clanPages.IsValidEncyclopediaItem(faction) && faction != _faction && FactionManager.IsAlliedWithFaction(_faction, faction))
                {
                    Allies.Add(new EncyclopediaFactionVM(faction));
                }
            }

            if (_faction.IsKingdomFaction)
            {
                foreach (var faction in Enumerable.ThenBy<IFaction, string>(Enumerable.OrderBy<IFaction, bool>(Campaign.Current.Factions, (IFaction x) => !x.IsKingdomFaction), (IFaction f) => f.Name.ToString()))
                {
                    if (clanPages.IsValidEncyclopediaItem(faction) && faction != _faction && faction.IsKingdomFaction && DiplomaticAgreementManager.Instance.HasNonAggressionPact(_faction as Kingdom, faction as Kingdom, out _))
                    {
                        NonAggressionPacts.Add(new EncyclopediaFactionVM(faction));
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
                return _alliesText;
            }
            set
            {
                if (value != _alliesText)
                {
                    _alliesText = value;
                    OnPropertyChanged("AlliesText");
                }
            }
        }

        [DataSourceProperty]
        public string NonAggressionPactsText
        {
            get
            {
                return _nonAggressionPactsText;
            }
            set
            {
                if (value != _nonAggressionPactsText)
                {
                    _nonAggressionPactsText = value;
                    OnPropertyChanged("NonAggressionPactsText");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFactionVM> Allies
        {
            get
            {
                return _allies;
            }
            set
            {
                if (value != _allies)
                {
                    _allies = value;
                    OnPropertyChanged("Allies");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFactionVM> NonAggressionPacts
        {
            get
            {
                return _nonAggressionPacts;
            }
            set
            {
                if (value != _nonAggressionPacts)
                {
                    _nonAggressionPacts = value;
                    OnPropertyChanged("NonAggressionPacts");
                }
            }
        }
    }
}
