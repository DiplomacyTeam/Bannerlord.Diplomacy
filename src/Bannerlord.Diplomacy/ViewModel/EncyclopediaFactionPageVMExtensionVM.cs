using Diplomacy.DiplomaticAction;
using Diplomacy.GauntletInterfaces;
using System.Linq;
using JetBrains.Annotations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.EncyclopediaItems;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    class EncyclopediaFactionPageVMExtensionVM : EncyclopediaFactionPageVM
    {
        private static readonly TextObject _TAllies = new("{=KqfNSsBE}Allies");
        private static readonly TextObject _TNonAggressionPacts = new(StringConstants.NonAggressionPacts);
        private static readonly TextObject _TFactions = new(StringConstants.Factions);

        private IFaction _faction;
        private string _alliesText;
        private string _nonAggressionPactsText;
        private MBBindingList<EncyclopediaFactionVM>? _allies;
        private MBBindingList<EncyclopediaFactionVM>? _nonAggressionPacts;

        public EncyclopediaFactionPageVMExtensionVM(EncyclopediaPageArgs args) : base(args)
        {
            _faction = (IFaction)Obj;
            _alliesText = _TAllies.ToString();
            _nonAggressionPactsText = _TNonAggressionPacts.ToString();
            FactionsText = _TFactions.ToString();
        }

        public override void RefreshValues()
        {
            _faction = (IFaction)Obj;
            _allies = new();
            _nonAggressionPacts = new();
            base.RefreshValues();
        }

        public override void Refresh()
        {
            IsLoadingOver = false;
            _allies = new();
            _nonAggressionPacts = new();

            var clanPages = Campaign.Current.EncyclopediaManager.GetPageOf(typeof(Clan));

            foreach (var f in Campaign.Current.Factions.Where(f => f != _faction).OrderBy(f => !f.IsKingdomFaction).ThenBy(f => f.Name.ToString()))
                if (clanPages.IsValidEncyclopediaItem(f) && FactionManager.IsAlliedWithFaction(_faction, f))
                    _allies.Add(new EncyclopediaFactionVM(f));

            if (_faction.IsKingdomFaction)
                foreach (var f in Kingdom.All.Cast<IFaction>().Where(f => f != _faction).OrderBy(f => f.Name.ToString()))
                    if (clanPages.IsValidEncyclopediaItem(f) && DiplomaticAgreementManager.HasNonAggressionPact((Kingdom)_faction, (Kingdom)f, out _))
                        _nonAggressionPacts.Add(new EncyclopediaFactionVM(f));

            base.Refresh();
        }

        [UsedImplicitly]
        public void OpenFactions()
        {
            new RebelFactionsInterface().ShowInterface(ScreenManager.TopScreen, (_faction as Kingdom)!);
        }

        [DataSourceProperty]
        public string FactionsText { get; set; }

        [DataSourceProperty]
        public string AlliesText
        {
            get => _alliesText;
            set
            {
                if (value != _alliesText)
                {
                    _alliesText = value;
                    OnPropertyChanged(nameof(AlliesText));
                }
            }
        }

        [DataSourceProperty]
        public string NonAggressionPactsText
        {
            get => _nonAggressionPactsText;
            set
            {
                if (value != _nonAggressionPactsText)
                {
                    _nonAggressionPactsText = value;
                    OnPropertyChanged(nameof(NonAggressionPactsText));
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFactionVM>? Allies
        {
            get => _allies;
            set
            {
                if (value != _allies)
                {
                    _allies = value;
                    OnPropertyChanged(nameof(Allies));
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFactionVM>? NonAggressionPacts
        {
            get => _nonAggressionPacts;
            set
            {
                if (value != _nonAggressionPacts)
                {
                    _nonAggressionPacts = value;
                    OnPropertyChanged(nameof(NonAggressionPacts));
                }
            }
        }
    }
}
