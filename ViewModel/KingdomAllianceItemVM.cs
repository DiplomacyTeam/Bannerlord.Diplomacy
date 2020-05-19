using DiplomacyFixes.Alliance;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Localization;

namespace DiplomacyFixes.ViewModel
{
    class KingdomAllianceItemVM : KingdomTruceItemVMExtensionVM
    {
        public KingdomAllianceItemVM(IFaction faction1, IFaction faction2, Action<KingdomDiplomacyItemVM> onSelection, Action<KingdomTruceItemVM> onAction) : base(faction1, faction2, onSelection, onAction) { }

        protected override void UpdateDiplomacyProperties()
        {
            base.UpdateDiplomacyProperties();
            this.ActionName = new TextObject("{=K4GraLTn}Break Alliance").ToString();
            this.InfluenceCost = 0;
            this.IsTruce = false;
        }

        protected override void UpdateActionAvailability()
        {
            base.UpdateActionAvailability();
            string breakAllianceException = AllianceConditions.CanBreakAllianceExceptions(this).FirstOrDefault()?.ToString();
            this.ActionHint = breakAllianceException != null ? new HintViewModel(breakAllianceException) : new HintViewModel();
            this.IsOptionAvailable = breakAllianceException == null;
        }
    }
}
