
using DiplomacyFixes.DiplomaticAction;
using DiplomacyFixes.ViewModel;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace DiplomacyFixes
{
    public class DiplomacyProperties : TaleWorlds.Library.ViewModel
    {

        private IFaction Faction1 { get; }
        private IFaction Faction2 { get; }

        public DiplomacyProperties(IFaction faction1, IFaction faction2)
        {
            this.Faction1 = faction1;
            this.Faction2 = faction2;
        }

        private MBBindingList<DiplomacyFactionRelationshipVM> _faction1Wars;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction1Allies;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction2Wars;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction2Allies;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction1Pacts;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction2Pacts;

        public void UpdateDiplomacyProperties()
        {
            if (this.Faction1Wars == null)
            {
                this.Faction1Wars = new MBBindingList<DiplomacyFactionRelationshipVM>();
            }
            if (this.Faction1Allies == null)
            {
                this.Faction1Allies = new MBBindingList<DiplomacyFactionRelationshipVM>();
            }
            if (this.Faction2Wars == null)
            {
                this.Faction2Wars = new MBBindingList<DiplomacyFactionRelationshipVM>();
            }
            if (this.Faction2Allies == null)
            {
                this.Faction2Allies = new MBBindingList<DiplomacyFactionRelationshipVM>();
            }
            if (this.Faction1Pacts == null)
            {
                this.Faction1Pacts = new MBBindingList<DiplomacyFactionRelationshipVM>();
            }
            if (this.Faction2Pacts == null)
            {
                this.Faction2Pacts = new MBBindingList<DiplomacyFactionRelationshipVM>();
            }

            this.Faction1Wars.Clear();
            this.Faction1Allies.Clear();
            this.Faction2Wars.Clear();
            this.Faction2Allies.Clear();
            this.Faction1Pacts.Clear();
            this.Faction2Pacts.Clear();

            AddWarRelationships(Faction1.Stances);
            AddWarRelationships(Faction2.Stances);

            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (FactionManager.IsAlliedWithFaction(kingdom, Faction1) && kingdom != Faction1)
                {
                    Faction1Allies.Add(new DiplomacyFactionRelationshipVM(kingdom));
                }

                if (FactionManager.IsAlliedWithFaction(kingdom, Faction2) && kingdom != Faction2)
                {
                    Faction2Allies.Add(new DiplomacyFactionRelationshipVM(kingdom));
                }

                if (DiplomaticAgreementManager.Instance.HasNonAggressionPact(kingdom, Faction1 as Kingdom))
                {
                    Faction1Pacts.Add(new DiplomacyFactionRelationshipVM(kingdom));
                }

                if (DiplomaticAgreementManager.Instance.HasNonAggressionPact(kingdom, Faction2 as Kingdom))
                {
                    Faction2Pacts.Add(new DiplomacyFactionRelationshipVM(kingdom));
                }
            }
        }

        private void AddWarRelationships(IEnumerable<StanceLink> stances)
        {

            foreach (StanceLink stanceLink in from x in stances
                                              where x.IsAtWar
                                              select x into w
                                              orderby w.Faction1.Name.ToString() + w.Faction2.Name.ToString()
                                              select w)
            {
                if (stanceLink.Faction1 is Kingdom && stanceLink.Faction2 is Kingdom && !stanceLink.Faction1.IsMinorFaction && !stanceLink.Faction2.IsMinorFaction)
                {

                    bool isFaction1War = stanceLink.Faction1 == Faction1 || stanceLink.Faction2 == Faction1;
                    bool isFaction2War = stanceLink.Faction1 == Faction2 || stanceLink.Faction2 == Faction2;

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
                return this._faction1Wars;
            }
            set
            {
                if (value != this._faction1Wars)
                {
                    this._faction1Wars = value;
                    base.OnPropertyChanged("Faction1Wars");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction1Allies
        {
            get
            {
                return this._faction1Allies;
            }
            set
            {
                if (value != this._faction1Allies)
                {
                    this._faction1Allies = value;
                    base.OnPropertyChanged("Faction1Allies");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction2Wars
        {
            get
            {
                return this._faction2Wars;
            }
            set
            {
                if (value != this._faction2Wars)
                {
                    this._faction2Wars = value;
                    base.OnPropertyChanged("Faction2Wars");
                }
            }
        }
        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction2Allies
        {
            get
            {
                return this._faction2Allies;
            }
            set
            {
                if (value != this._faction2Allies)
                {
                    this._faction2Allies = value;
                    base.OnPropertyChanged("Faction2Allies");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction1Pacts
        {
            get
            {
                return this._faction1Pacts;
            }
            set
            {
                if (value != this._faction1Pacts)
                {
                    this._faction1Pacts = value;
                    base.OnPropertyChanged("Faction1Pacts");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<DiplomacyFactionRelationshipVM> Faction2Pacts
        {
            get
            {
                return this._faction2Pacts;
            }
            set
            {
                if (value != this._faction2Pacts)
                {
                    this._faction2Pacts = value;
                    base.OnPropertyChanged("Faction2Pacts");
                }
            }
        }

    }
}
