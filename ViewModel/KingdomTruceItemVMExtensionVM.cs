using DiplomacyFixes.Alliance;
using DiplomacyFixes.Messengers;
using DiplomacyFixes.WarPeace;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.ViewModel
{
    public class KingdomTruceItemVMExtensionVM : KingdomTruceItemVM
    {
        // private static string INFLUENCE_COST = "Influence Cost: {0}";

        public KingdomTruceItemVMExtensionVM(IFaction faction1, IFaction faction2, Action<KingdomDiplomacyItemVM> onSelection, Action<KingdomTruceItemVM> onAction) : base(faction1, faction2, onSelection, onAction)
        {
            this.SendMessengerActionName = new TextObject("{=cXfcwzPp}Send Messenger").ToString();
            this.AllianceActionName = new TextObject("{=0WPWbx70}Form Alliance").ToString();
            this.InfluenceCost = (int)DiplomacyCostCalculator.DetermineInfluenceCostForDeclaringWar(Faction1 as Kingdom);
            UpdateDiplomacyProperties();
        }

        protected override void UpdateDiplomacyProperties()
        {
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

        protected virtual void UpdateActionAvailability()
        {
            this.IsMessengerAvailable = MessengerManager.CanSendMessengerWithInfluenceCost(Faction2Leader.Hero, this.SendMessengerInfluenceCost);
            this.IsOptionAvailable = WarAndPeaceConditions.CanDeclareWarExceptions(this).IsEmpty();
            string allianceException = AllianceConditions.CanFormAllianceExceptions(this, true).FirstOrDefault()?.ToString();
            this.IsAllianceAvailable = allianceException == null;
            string declareWarException = WarAndPeaceConditions.CanDeclareWarExceptions(this).FirstOrDefault()?.ToString();
            this.ActionHint = declareWarException != null ? new HintViewModel(declareWarException) : new HintViewModel();
            this.AllianceHint = allianceException != null ? new HintViewModel(allianceException) : new HintViewModel();
            this.AllianceInfluenceCost = (int)DiplomacyCostCalculator.DetermineInfluenceCostForFormingAlliance(Faction1 as Kingdom, Faction2 as Kingdom, true);
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

        protected void FormAlliance()
        {
            DeclareAllianceAction.Apply(this.Faction1 as Kingdom, this.Faction2 as Kingdom, true);
        }

        [DataSourceProperty]
        public bool IsAllianceAvailable
        {
            get { 
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
        public string SendMessengerActionName { get; private set; }
        [DataSourceProperty]
        public string AllianceActionName { get; }
        [DataSourceProperty]
        public bool IsGoldCostVisible { get; } = false;

        private bool _isOptionAvailable;
        private bool _isMessengerAvailable;
        private bool _isAllianceAvailable;
        private int _allianceInfluenceCost;
        private HintViewModel _allianceHint;
        private HintViewModel _actionHint;
    }
}
