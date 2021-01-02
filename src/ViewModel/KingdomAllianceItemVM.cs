using Diplomacy.DiplomaticAction.Alliance;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    class KingdomAllianceItemVM : KingdomTruceItemVMExtensionVM
    {
        public KingdomAllianceItemVM(IFaction faction1, IFaction faction2, Action<KingdomDiplomacyItemVM> onSelection, Action<KingdomTruceItemVM> onAction) : base(faction1, faction2, onSelection, onAction) { }

        protected override void UpdateDiplomacyProperties()
        {
            base.UpdateDiplomacyProperties();
            ActionName = new TextObject("{=K4GraLTn}Break Alliance").ToString();
            InfluenceCost = 0;
            IsTruce = false;
        }

        protected override void UpdateActionAvailability()
        {
            base.UpdateActionAvailability();
            var breakAllianceException = BreakAllianceConditions.Instance.CanApplyExceptions(this, true).FirstOrDefault()?.ToString();
            ActionHint = breakAllianceException is not null ? new HintViewModel(breakAllianceException) : new HintViewModel();
            IsOptionAvailable = breakAllianceException is null;
        }

        protected override void ExecuteExecutiveAction()
        {
            BreakAllianceAction.Apply(Faction1 as Kingdom, Faction2 as Kingdom);
        }
    }
}
