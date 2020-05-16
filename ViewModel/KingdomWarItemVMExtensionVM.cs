using DiplomacyFixes.Messengers;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
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
        public string SendMessengerActionName { get; }

        private bool _isOptionAvailable;
        private int _goldCost;
        private bool _isMessengerAvailable;
    }
}
