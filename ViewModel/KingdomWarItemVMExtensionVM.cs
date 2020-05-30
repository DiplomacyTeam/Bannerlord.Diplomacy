using DiplomacyFixes.Messengers;
using DiplomacyFixes.WarPeace;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.ViewModel
{
    public class KingdomWarItemVMExtensionVM : KingdomWarItemVM
    {
        public KingdomWarItemVMExtensionVM(CampaignWar war, Action<KingdomWarItemVM> onSelect, Action<KingdomWarItemVM> onAction) : base(war, onSelect, onAction)
        {
            this.SendMessengerActionName = new TextObject("{=cXfcwzPp}Send Messenger").ToString();
            this.InfluenceCost = (int)DiplomacyCostCalculator.DetermineInfluenceCostForMakingPeace(Faction1 as Kingdom);
            this.GoldCost = DiplomacyCostCalculator.DetermineGoldCostForMakingPeace(this.Faction1 as Kingdom, this.Faction2 as Kingdom);
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

            this.Faction1Wars.Clear();
            this.Faction1Allies.Clear();
            this.Faction2Wars.Clear();
            this.Faction2Allies.Clear();

            foreach (CampaignWar campaignWar in from w in FactionManager.Instance.CampaignWars
                                                orderby w.Side1[0].Name.ToString()
                                                select w)
            {
                if (campaignWar.Side1[0] is Kingdom && campaignWar.Side2[0] is Kingdom && !campaignWar.Side1[0].IsMinorFaction && !campaignWar.Side2[0].IsMinorFaction)
                {

                    bool isFaction1War = (campaignWar.Side1[0] == Faction1 || campaignWar.Side2[0] == Faction1);
                    bool isFaction2War = (campaignWar.Side1[0] == Faction2 || campaignWar.Side2[0] == Faction2);
                    if (isFaction1War && isFaction2War)
                    {
                        continue;
                    }

                    if (isFaction1War)
                    {
                        Faction1Wars.Add(new DiplomacyFactionRelationshipVM(campaignWar.Side1[0] == Faction1 ? campaignWar.Side2[0] : campaignWar.Side1[0]));
                    }
                    if (isFaction2War)
                    {
                        Faction2Wars.Add(new DiplomacyFactionRelationshipVM(campaignWar.Side1[0] == Faction2 ? campaignWar.Side2[0] : campaignWar.Side1[0]));
                    }
                }
            }

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
            }

            base.UpdateDiplomacyProperties();
            UpdateActionAvailability();

            if (Settings.Instance.EnableWarExhaustion)
            {
                this.Stats.Insert(1, new KingdomWarComparableStatVM(
                    (int)Math.Ceiling(WarExhaustionManager.Instance.GetWarExhaustion((Kingdom)this.Faction1, (Kingdom)this.Faction2)),
                    (int)Math.Ceiling(WarExhaustionManager.Instance.GetWarExhaustion((Kingdom)this.Faction2, (Kingdom)this.Faction1)),
                    new TextObject("{=XmVTQ0bH}War Exhaustion"), this._faction1Color, this._faction2Color, null));
            }
        }

        private void UpdateActionAvailability()
        {
            this.IsMessengerAvailable = MessengerManager.CanSendMessengerWithInfluenceCost(Faction2Leader.Hero, this.SendMessengerInfluenceCost);
            this.IsOptionAvailable = WarAndPeaceConditions.CanMakePeaceExceptions(this).IsEmpty();
            string makePeaceException = WarAndPeaceConditions.CanMakePeaceExceptions(this).FirstOrDefault()?.ToString();
            this.ActionHint = makePeaceException != null ? new HintViewModel(makePeaceException) : new HintViewModel();
        }

        protected override void OnSelect()
        {
            base.OnSelect();
            UpdateActionAvailability();
        }

        protected void SendMessenger()
        {
            Events.Instance.OnMessengerSent(Faction2Leader.Hero);
            this.UpdateDiplomacyProperties();
        }

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
        public int InfluenceCost { get; }

        [DataSourceProperty]
        public int GoldCost
        {
            get
            {
                return this._goldCost;
            }
            set
            {
                if (value != this._goldCost)
                {
                    this._goldCost = value;
                    base.OnPropertyChanged("GoldCost");
                }
            }
        }


        [DataSourceProperty]
        public bool IsGoldCostVisible { get; } = true;

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
        private HintViewModel _actionHint;

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
        public string SendMessengerActionName { get; }

        private bool _isOptionAvailable;
        private int _goldCost;
        private bool _isMessengerAvailable;

        private MBBindingList<DiplomacyFactionRelationshipVM> _faction1Wars;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction1Allies;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction2Wars;
        private MBBindingList<DiplomacyFactionRelationshipVM> _faction2Allies;
    }
}
