using Diplomacy.DiplomaticAction;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    public class DiplomacyPropertiesVM : TaleWorlds.Library.ViewModel
    {

        private IFaction Faction1 { get; }
        private IFaction Faction2 { get; }

        public DiplomacyPropertiesVM(IFaction faction1, IFaction faction2)
        {
            Faction1 = faction1;
            Faction2 = faction2;
        }

        private MBBindingList<DiplomacyFactionRelationshipVM> _faction1Wars;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction1Allies;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction2Wars;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction2Allies;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction1Pacts;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction2Pacts;

        public void UpdateDiplomacyProperties()
        {
            if (Faction1Wars is null)
            {
                Faction1Wars = new MBBindingList<DiplomacyFactionRelationshipVM>();
            }
            if (Faction1Allies is null)
            {
                Faction1Allies = new MBBindingList<DiplomacyFactionRelationshipVM>();
            }
            if (Faction2Wars is null)
            {
                Faction2Wars = new MBBindingList<DiplomacyFactionRelationshipVM>();
            }
            if (Faction2Allies is null)
            {
                Faction2Allies = new MBBindingList<DiplomacyFactionRelationshipVM>();
            }
            if (Faction1Pacts is null)
            {
                Faction1Pacts = new MBBindingList<DiplomacyFactionRelationshipVM>();
            }
            if (Faction2Pacts is null)
            {
                Faction2Pacts = new MBBindingList<DiplomacyFactionRelationshipVM>();
            }

            Faction1Wars.Clear();
            Faction1Allies.Clear();
            Faction2Wars.Clear();
            Faction2Allies.Clear();
            Faction1Pacts.Clear();
            Faction2Pacts.Clear();

            AddWarRelationships(Faction1.Stances);
            AddWarRelationships(Faction2.Stances);

            foreach (var kingdom in Kingdom.All)
            {
                if (FactionManager.IsAlliedWithFaction(kingdom, Faction1) && kingdom != Faction1)
                {
                    Faction1Allies.Add(new DiplomacyFactionRelationshipVM(kingdom));
                }

                if (FactionManager.IsAlliedWithFaction(kingdom, Faction2) && kingdom != Faction2)
                {
                    Faction2Allies.Add(new DiplomacyFactionRelationshipVM(kingdom));
                }

                AddNonAggressionPactRelationships(kingdom, Faction1, Faction1Pacts);
                AddNonAggressionPactRelationships(kingdom, Faction2, Faction2Pacts);
            }
        }

        private void AddNonAggressionPactRelationships(Kingdom kingdom, IFaction faction, MBBindingList<DiplomacyFactionRelationshipVM> FactionPacts)
        {
            if (DiplomaticAgreementManager.Instance.HasNonAggressionPact(kingdom, faction as Kingdom, out var pact))
            {
                var textObject = new TextObject(StringConstants.DaysRemaining);
                textObject.SetTextVariable("DAYS_LEFT", (int)Math.Round(pact.EndDate.RemainingDaysFromNow));
                FactionPacts.Add(new DiplomacyFactionRelationshipVM(kingdom, new HintViewModel(textObject.ToString())));
            }
        }

        private void AddWarRelationships(IEnumerable<StanceLink> stances)
        {

            foreach (var stanceLink in from x in stances
                                              where x.IsAtWar
                                              select x into w
                                              orderby w.Faction1.Name.ToString() + w.Faction2.Name.ToString()
                                              select w)
            {
                if (stanceLink.Faction1 is Kingdom && stanceLink.Faction2 is Kingdom && !stanceLink.Faction1.IsMinorFaction && !stanceLink.Faction2.IsMinorFaction)
                {

                    var isFaction1War = stanceLink.Faction1 == Faction1 || stanceLink.Faction2 == Faction1;
                    var isFaction2War = stanceLink.Faction1 == Faction2 || stanceLink.Faction2 == Faction2;

                    if (isFaction1War && isFaction2War)
                    {
                        // make sure we only add the shared war once
                        if (Faction1Wars.Any(war => war.Faction == Faction2))
                        {
                            continue;
                        }
                    }

                    if (isFaction1War)
                    {
                        Faction1Wars.Add(new DiplomacyFactionRelationshipVM(stanceLink.Faction1 == Faction1 ? stanceLink.Faction2 : stanceLink.Faction1));
                    }
                    if (isFaction2War)
                    {
                        Faction2Wars.Add(new DiplomacyFactionRelationshipVM(stanceLink.Faction1 == Faction2 ? stanceLink.Faction2 : stanceLink.Faction1));
                    }
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction1Wars
        {
            get
            {
                return _faction1Wars;
            }
            set
            {
                if (value != _faction1Wars)
                {
                    _faction1Wars = value;
                    OnPropertyChanged("Faction1Wars");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction1Allies
        {
            get
            {
                return _faction1Allies;
            }
            set
            {
                if (value != _faction1Allies)
                {
                    _faction1Allies = value;
                    OnPropertyChanged("Faction1Allies");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction2Wars
        {
            get
            {
                return _faction2Wars;
            }
            set
            {
                if (value != _faction2Wars)
                {
                    _faction2Wars = value;
                    OnPropertyChanged("Faction2Wars");
                }
            }
        }
        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction2Allies
        {
            get
            {
                return _faction2Allies;
            }
            set
            {
                if (value != _faction2Allies)
                {
                    _faction2Allies = value;
                    OnPropertyChanged("Faction2Allies");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction1Pacts
        {
            get
            {
                return _faction1Pacts;
            }
            set
            {
                if (value != _faction1Pacts)
                {
                    _faction1Pacts = value;
                    OnPropertyChanged("Faction1Pacts");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction2Pacts
        {
            get
            {
                return _faction2Pacts;
            }
            set
            {
                if (value != _faction2Pacts)
                {
                    _faction2Pacts = value;
                    OnPropertyChanged("Faction2Pacts");
                }
            }
        }

    }
}
