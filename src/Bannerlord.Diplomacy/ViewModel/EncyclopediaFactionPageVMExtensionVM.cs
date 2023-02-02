﻿using Diplomacy.DiplomaticAction;
using Diplomacy.GauntletInterfaces;

using JetBrains.Annotations;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;

namespace Diplomacy.ViewModel
{
    class EncyclopediaFactionPageVMExtensionVM : EncyclopediaFactionPageVM
    {
        private static readonly TextObject _TAllies = new(StringConstants.Allies);
        private static readonly TextObject _TNonAggressionPacts = new(StringConstants.NonAggressionPacts);
        private static readonly TextObject _TFactions = new(StringConstants.Factions);
        private readonly RebelFactionsInterface _rebelFactionsInterface;

        private IFaction _faction;
        private string _alliesText;
        private string _nonAggressionPactsText;
        private MBBindingList<EncyclopediaFactionVM>? _allies;
        private MBBindingList<EncyclopediaFactionVM>? _nonAggressionPacts;

        public EncyclopediaFactionPageVMExtensionVM(EncyclopediaPageArgs args) : base(args)
        {
            _rebelFactionsInterface = new RebelFactionsInterface();
            _faction = (IFaction) Obj;
            _alliesText = _TAllies.ToString();
            _nonAggressionPactsText = _TNonAggressionPacts.ToString();
            FactionsText = _TFactions.ToString();
        }

        public override void RefreshValues()
        {
            _faction = (IFaction) Obj;
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
                    if (clanPages.IsValidEncyclopediaItem(f) && DiplomaticAgreementManager.HasNonAggressionPact((Kingdom) _faction, (Kingdom) f, out _))
                        _nonAggressionPacts.Add(new EncyclopediaFactionVM(f));

            base.Refresh();
        }

        [UsedImplicitly]
        public void OpenFactions() => _rebelFactionsInterface.ShowInterface(ScreenManager.TopScreen, (_faction as Kingdom)!);

        [DataSourceProperty]
        public string FactionsText { get; set; }

        [DataSourceProperty]
        public string AlliesText { get => _alliesText; set => SetField(ref _alliesText, value, nameof(AlliesText)); }

        [DataSourceProperty]
        public string NonAggressionPactsText { get => _nonAggressionPactsText; set => SetField(ref _nonAggressionPactsText, value, nameof(NonAggressionPactsText)); }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFactionVM>? Allies { get => _allies; set => SetField(ref _allies, value, nameof(Allies)); }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFactionVM>? NonAggressionPacts { get => _nonAggressionPacts; set => SetField(ref _nonAggressionPacts, value, nameof(NonAggressionPacts)); }
    }
}