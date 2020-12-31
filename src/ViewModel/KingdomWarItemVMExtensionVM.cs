using Diplomacy.Costs;
using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.Messengers;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    public class KingdomWarItemVMExtensionVM : KingdomWarItemVM
    {

        public KingdomWarItemVMExtensionVM(StanceLink stanceLink, Action<KingdomWarItemVM> onSelect, Action<KingdomWarItemVM> onAction) : base(stanceLink, onSelect, onAction)
        {
            this.SendMessengerActionName = new TextObject("{=cXfcwzPp}Send Messenger").ToString();
            HybridCost costForMakingPeace = DiplomacyCostCalculator.DetermineCostForMakingPeace(Faction1 as Kingdom, Faction2 as Kingdom, true);
            this.InfluenceCost = (int)costForMakingPeace.InfluenceCost.Value;
            this.GoldCost = (int)costForMakingPeace.GoldCost.Value;
            this.ActionName = GameTexts.FindText("str_kingdom_propose_peace_action", null).ToString();
            this.AllianceText = new TextObject("{=zpNalMeA}Alliances").ToString();
            this.WarsText = new TextObject("{=y5tXjbLK}Wars").ToString();
            this.PactsText = new TextObject("{=noWHMN1W}Non-Aggression Pacts").ToString();
            UpdateDiplomacyProperties();
        }

        protected override void UpdateDiplomacyProperties()
        {
            if (this.DiplomacyProperties == null)
            {
                this.DiplomacyProperties = new DiplomacyPropertiesVM(Faction1, Faction2);
            }
            this.DiplomacyProperties.UpdateDiplomacyProperties();

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
        private void ExecuteExecutiveAction()
        {
            KingdomPeaceAction.ApplyPeace(Faction1 as Kingdom, Faction2 as Kingdom, forcePlayerCharacterCosts: true);
        }

        private void UpdateActionAvailability()
        {
            this.IsMessengerAvailable = MessengerManager.CanSendMessengerWithCost(Faction2Leader.Hero, DiplomacyCostCalculator.DetermineCostForSendingMessenger());
            this.IsOptionAvailable = MakePeaceConditions.Instance.CanApplyExceptions(this, true).IsEmpty();
            string makePeaceException = MakePeaceConditions.Instance.CanApplyExceptions(this, true).FirstOrDefault()?.ToString();
            this.ActionHint = makePeaceException != null ? new HintViewModel(makePeaceException) : new HintViewModel();
        }

        protected void SendMessenger()
        {
            Events.Instance.OnMessengerSent(Faction2Leader.Hero);
            this.UpdateDiplomacyProperties();
        }

        [DataSourceProperty]
        public string ActionName { get; protected set; }

        [DataSourceProperty]
        public int SendMessengerInfluenceCost { get; } = (int)DiplomacyCostCalculator.DetermineCostForSendingMessenger().Value;

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
        public string SendMessengerActionName { get; }

        [DataSourceProperty]
        public string AllianceText { get; }
        [DataSourceProperty]
        public string WarsText { get; }
        [DataSourceProperty]
        public string PactsText { get; }
        [DataSourceProperty]
        public DiplomacyPropertiesVM DiplomacyProperties { get; private set; }

        private bool _isOptionAvailable;
        private int _goldCost;
        private bool _isMessengerAvailable;
    }
}
