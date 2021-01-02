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
            SendMessengerActionName = new TextObject("{=cXfcwzPp}Send Messenger").ToString();
            var costForMakingPeace = DiplomacyCostCalculator.DetermineCostForMakingPeace(Faction1 as Kingdom, Faction2 as Kingdom, true);
            InfluenceCost = (int)costForMakingPeace.InfluenceCost.Value;
            GoldCost = (int)costForMakingPeace.GoldCost.Value;
            ActionName = GameTexts.FindText("str_kingdom_propose_peace_action", null).ToString();
            AllianceText = new TextObject("{=zpNalMeA}Alliances").ToString();
            WarsText = new TextObject("{=y5tXjbLK}Wars").ToString();
            PactsText = new TextObject("{=noWHMN1W}Non-Aggression Pacts").ToString();
            UpdateDiplomacyProperties();
        }

        protected override void UpdateDiplomacyProperties()
        {
            if (DiplomacyProperties is null)
            {
                DiplomacyProperties = new DiplomacyPropertiesVM(Faction1, Faction2);
            }
            DiplomacyProperties.UpdateDiplomacyProperties();

            base.UpdateDiplomacyProperties();
            UpdateActionAvailability();

            if (Settings.Instance.EnableWarExhaustion)
            {
                Stats.Insert(1, new KingdomWarComparableStatVM(
                    (int)Math.Ceiling(WarExhaustionManager.Instance.GetWarExhaustion((Kingdom)Faction1, (Kingdom)Faction2)),
                    (int)Math.Ceiling(WarExhaustionManager.Instance.GetWarExhaustion((Kingdom)Faction2, (Kingdom)Faction1)),
                    new TextObject("{=XmVTQ0bH}War Exhaustion"), _faction1Color, _faction2Color, 100, null));
            }
        }
        private void ExecuteExecutiveAction()
        {
            KingdomPeaceAction.ApplyPeace(Faction1 as Kingdom, Faction2 as Kingdom, forcePlayerCharacterCosts: true);
        }

        private void UpdateActionAvailability()
        {
            IsMessengerAvailable = MessengerManager.CanSendMessengerWithCost(Faction2Leader.Hero, DiplomacyCostCalculator.DetermineCostForSendingMessenger());
            IsOptionAvailable = MakePeaceConditions.Instance.CanApplyExceptions(this, true).IsEmpty();
            var makePeaceException = MakePeaceConditions.Instance.CanApplyExceptions(this, true).FirstOrDefault()?.ToString();
            ActionHint = makePeaceException is not null ? new HintViewModel(makePeaceException) : new HintViewModel();
        }

        protected void SendMessenger()
        {
            Events.Instance.OnMessengerSent(Faction2Leader.Hero);
            UpdateDiplomacyProperties();
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
                return _isMessengerAvailable;
            }
            set
            {
                if (value != _isMessengerAvailable)
                {
                    _isMessengerAvailable = value;
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
                return _goldCost;
            }
            set
            {
                if (value != _goldCost)
                {
                    _goldCost = value;
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
                return _isOptionAvailable;
            }
            set
            {
                if (value != _isOptionAvailable)
                {
                    _isOptionAvailable = value;
                    base.OnPropertyChanged("IsOptionAvailable");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel ActionHint
        {
            get
            {
                return _actionHint;
            }
            set
            {
                if (value != _actionHint)
                {
                    _actionHint = value;
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
