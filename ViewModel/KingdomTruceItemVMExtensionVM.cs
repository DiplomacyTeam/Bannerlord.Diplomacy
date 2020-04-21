using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace DiplomacyFixes.ViewModel
{
    public class KingdomTruceItemVMExtensionVM : KingdomTruceItemVM
    {
        private static string INFLUENCE_COST = "Influence Cost: {0}";

        public KingdomTruceItemVMExtensionVM(IFaction faction1, IFaction faction2, Action<KingdomDiplomacyItemVM> onSelection, Action<KingdomTruceItemVM> onAction) : base(faction1, faction2, onSelection, onAction)
        {
            _isOptionAvailable = true;
            _influenceCost = String.Format(INFLUENCE_COST, 0);
            UpdateDiplomacyProperties();
        }

        protected override void UpdateDiplomacyProperties()
        {
            base.UpdateDiplomacyProperties();
            this._isOptionAvailable = WarAndPeaceConditions.canDeclareWarExceptions(this).IsEmpty();
            this._influenceCost = String.Format(INFLUENCE_COST, (int)CostCalculator.determineInfluenceCostForDeclaringWar());
        }

        [DataSourceProperty]
        public string InfluenceCost
        {
            get
            {
                return this._influenceCost;
            }
            set
            {
                if (value != this._influenceCost)
                {
                    this._influenceCost = value;
                    base.OnPropertyChanged("InfluenceCost");
                }
            }
        }

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

        private bool _isOptionAvailable;
        private string _influenceCost;
    }
}
