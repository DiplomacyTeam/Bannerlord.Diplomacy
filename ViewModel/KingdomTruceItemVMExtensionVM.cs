using DiplomacyFixes.DiplomaticAction;
using DiplomacyFixes.DiplomaticAction.Alliance;
using DiplomacyFixes.DiplomaticAction.NonAggressionPact;
using DiplomacyFixes.DiplomaticAction.WarPeace;
using DiplomacyFixes.Messengers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.ViewModel
{
    public class KingdomTruceItemVMExtensionVM : KingdomTruceItemVM
    {

        public KingdomTruceItemVMExtensionVM(IFaction faction1, IFaction faction2, Action<KingdomDiplomacyItemVM> onSelection, Action<KingdomTruceItemVM> onAction) : base(faction1, faction2, onSelection, onAction)
        {
            this.SendMessengerActionName = new TextObject("{=cXfcwzPp}Send Messenger").ToString();
            this.AllianceActionName = new TextObject("{=0WPWbx70}Form Alliance").ToString();
            this.InfluenceCost = (int)DiplomacyCostCalculator.DetermineInfluenceCostForDeclaringWar(Faction1 as Kingdom);
            this.ActionName = GameTexts.FindText("str_kingdom_declate_war_action", null).ToString();
            this.NonAggressionPactActionName = new TextObject("{=9pY0NQrk}Form Pact").ToString();
            
            TextObject textObject = new TextObject("{=9zlQNtlX}Form a non-aggression pact lasting {PACT_DURATION_DAYS} days.");
            textObject.SetTextVariable("PACT_DURATION_DAYS", Settings.Instance.NonAggressionPactDuration);
            this.NonAggressionPactHelpText = textObject.ToString();

            this.AllianceText = new TextObject("{=zpNalMeA}Alliances").ToString();
            this.WarsText = new TextObject("{=y5tXjbLK}Wars").ToString();
            this.PactsText = new TextObject("{=noWHMN1W}Non-Aggression Pacts").ToString();
            UpdateDiplomacyProperties();
        }

        protected override void UpdateDiplomacyProperties()
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


            base.UpdateDiplomacyProperties();
            UpdateActionAvailability();


            if (Settings.Instance.EnableWarExhaustion)
            {
                this.Stats.Insert(1, new KingdomWarComparableStatVM(
                    (int)Math.Ceiling(WarExhaustionManager.Instance.GetWarExhaustion((Kingdom)this.Faction1, (Kingdom)this.Faction2)),
                    (int)Math.Ceiling(WarExhaustionManager.Instance.GetWarExhaustion((Kingdom)this.Faction2, (Kingdom)this.Faction1)),
                    new TextObject("{=XmVTQ0bH}War Exhaustion"), this._faction1Color, this._faction2Color, 100, null));
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
                        continue;
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

        protected virtual void ExecuteExecutiveAction()
        {
            float influenceCost = DiplomacyCostCalculator.DetermineInfluenceCostForDeclaringWar(Faction1 as Kingdom);
            DiplomacyCostManager.deductInfluenceFromPlayerClan(influenceCost);
            DeclareWarAction.Apply(Faction1, Faction2);
        }

        protected virtual void UpdateActionAvailability()
        {
            this.IsMessengerAvailable = MessengerManager.CanSendMessengerWithCost(Faction2Leader.Hero, DiplomacyCostCalculator.DetermineCostForSendingMessenger());
            this.IsOptionAvailable = WarAndPeaceConditions.CanDeclareWarExceptions(this).IsEmpty();
            string allianceException = AllianceConditions.CanFormAllianceExceptions(this, true).FirstOrDefault()?.ToString();
            this.IsAllianceAvailable = allianceException == null;
            string declareWarException = WarAndPeaceConditions.CanDeclareWarExceptions(this).FirstOrDefault()?.ToString();
            this.ActionHint = declareWarException != null ? new HintViewModel(declareWarException) : new HintViewModel();
            this.AllianceHint = allianceException != null ? new HintViewModel(allianceException) : new HintViewModel();
            string nonAggressionPactException = NonAggressionPactConditions.Instance.CanExecuteActionExceptions(this, true).FirstOrDefault()?.ToString();
            this.IsNonAggressionPactAvailable = nonAggressionPactException == null;
            this.NonAggressionPactHint = nonAggressionPactException != null ? new HintViewModel(nonAggressionPactException) : new HintViewModel();
            this.AllianceInfluenceCost = (int)DiplomacyCostCalculator.DetermineCostForFormingAlliance(Faction1 as Kingdom, Faction2 as Kingdom, true).Value;
            this.NonAggressionPactInfluenceCost = (int)DiplomacyCostCalculator.DetermineCostForFormingNonAggressionPact(Faction1 as Kingdom, true).Value;
        }

        protected void SendMessenger()
        {
            Events.Instance.OnMessengerSent(Faction2Leader.Hero);
            this.UpdateDiplomacyProperties();
        }

        protected void FormAlliance()
        {
            DeclareAllianceAction.Apply(this.Faction1 as Kingdom, this.Faction2 as Kingdom, true);
        }

        protected void ProposeNonAggressionPact()
        {
            FormNonAggressionPactAction.Apply(this.Faction1 as Kingdom, this.Faction2 as Kingdom, true);
            this.UpdateDiplomacyProperties();
        }

        [DataSourceProperty]
        public bool IsAllianceAvailable
        {
            get
            {
                return this._isAllianceAvailable;
            }
            set
            {
                if (value != this._isAllianceAvailable)
                {
                    this._isAllianceAvailable = value;
                    base.OnPropertyChanged("isAllianceAvailable");
                }
            }
        }

        [DataSourceProperty]
        public int AllianceInfluenceCost
        {
            get
            {
                return this._allianceInfluenceCost;
            }
            set
            {
                if (value != this._allianceInfluenceCost)
                {
                    this._allianceInfluenceCost = value;
                    base.OnPropertyChanged("AllianceInfluenceCost");
                }
            }
        }

        [DataSourceProperty]
        public bool IsNonAggressionPactAvailable
        {
            get
            {
                return this._isNonAggressionPactAvailable;
            }
            set
            {
                if (value != this._isNonAggressionPactAvailable)
                {
                    this._isNonAggressionPactAvailable = value;
                    base.OnPropertyChanged("IsNonAggressionPactAvailable");
                }
            }
        }

        [DataSourceProperty]
        public int NonAggressionPactInfluenceCost
        {
            get
            {
                return this._nonAggressionPactInfluenceCost;
            }
            set
            {
                if (value != this._nonAggressionPactInfluenceCost)
                {
                    this._nonAggressionPactInfluenceCost = value;
                    base.OnPropertyChanged("NonAggressionPactInfluenceCost");
                }
            }
        }

        [DataSourceProperty]
        public string ActionName { get; protected set; }

        [DataSourceProperty]
        public string NonAggressionPactActionName { get; }
        [DataSourceProperty]
        public string AllianceText { get; }
        [DataSourceProperty]
        public string WarsText { get; }
        [DataSourceProperty]
        public string PactsText { get; }
        [DataSourceProperty]
        public int SendMessengerInfluenceCost { get; } = (int)DiplomacyCostCalculator.DetermineInfluenceCostForSendingMessenger();

        [DataSourceProperty]
        public bool IsMessengerAvailable
        {
            get
            {
                return this._isMessengerAvailable;
            }
            set
            {
                if (value != this._isMessengerAvailable)
                {
                    this._isMessengerAvailable = value;
                    base.OnPropertyChanged("IsMessengerAvailable");
                }
            }
        }

        [DataSourceProperty]
        public int InfluenceCost { get; protected set; }

        [DataSourceProperty]
        public bool IsOptionAvailable
        {
            get
            {
                return this._isOptionAvailable;
            }
            set
            {
                if (value != this._isOptionAvailable)
                {
                    this._isOptionAvailable = value;
                    base.OnPropertyChanged("IsOptionAvailable");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel ActionHint
        {
            get
            {
                return this._actionHint;
            }
            set
            {
                if (value != this._actionHint)
                {
                    this._actionHint = value;
                    base.OnPropertyChanged("ActionHint");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel AllianceHint
        {
            get
            {
                return this._allianceHint;
            }
            set
            {
                if (value != this._allianceHint)
                {
                    this._allianceHint = value;
                    base.OnPropertyChanged("AllianceHint");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel NonAggressionPactHint
        {
            get
            {
                return this._nonAggressionPactHint;
            }
            set
            {
                if (value != this._nonAggressionPactHint)
                {
                    this._nonAggressionPactHint = value;
                    base.OnPropertyChanged("NonAggressionPactHint");
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

        [DataSourceProperty]
        public string SendMessengerActionName { get; private set; }
        [DataSourceProperty]
        public string AllianceActionName { get; }
        [DataSourceProperty]
        public bool IsGoldCostVisible { get; } = false;

        [DataSourceProperty]
        public ImageIdentifierVM Faction1Image { get; private set; }

        [DataSourceProperty]
        public ImageIdentifierVM Faction2Image { get; private set; }
        [DataSourceProperty]
        public string NonAggressionPactHelpText { get; }

        private bool _isOptionAvailable;
        private bool _isMessengerAvailable;
        private bool _isAllianceAvailable;
        private bool _isNonAggressionPactAvailable;
        private int _allianceInfluenceCost;
        private int _nonAggressionPactInfluenceCost;
        private HintViewModel _allianceHint;
        private HintViewModel _nonAggressionPactHint;
        private HintViewModel _actionHint;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction1Wars;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction1Allies;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction2Wars;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction2Allies;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction1Pacts;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction2Pacts;
    }
}
