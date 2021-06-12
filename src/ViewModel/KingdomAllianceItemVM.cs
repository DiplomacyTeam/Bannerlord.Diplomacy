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
        private static readonly TextObject _TBreakAlliance = new TextObject("{=K4GraLTn}Break Alliance");

        public KingdomAllianceItemVM(IFaction faction1, IFaction faction2, Action<KingdomDiplomacyItemVM> onSelection, Action<KingdomTruceItemVM> onAction) : base(faction1, faction2, onSelection, onAction) { }

        protected override void UpdateDiplomacyProperties()
        {
            ActionName = _TBreakAlliance.ToString();
            base.UpdateDiplomacyProperties();
            InfluenceCost = 0;
        }

        protected override void UpdateActionAvailability()
        {
            base.UpdateActionAvailability();
            var breakAllianceException = BreakAllianceConditions.Instance.CanApplyExceptions(this, true).FirstOrDefault();
            ActionHint = breakAllianceException is not null ? Compat.HintViewModel.Create(breakAllianceException) : new HintViewModel();
            IsOptionAvailable = breakAllianceException is null;
        }

        protected override void ExecuteExecutiveAction() => BreakAllianceAction.Apply((Kingdom)Faction1, (Kingdom)Faction2);
    }
}
