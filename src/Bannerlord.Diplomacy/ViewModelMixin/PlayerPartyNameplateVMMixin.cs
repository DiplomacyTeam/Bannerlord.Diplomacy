using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using Diplomacy.Events;

using JetBrains.Annotations;

using SandBox.ViewModelCollection.Nameplate;

using TaleWorlds.CampaignSystem;

namespace Diplomacy.ViewModelMixin
{
    [ViewModelMixin]
    [UsedImplicitly]
    public class PlayerPartyNameplateVMMixin : BaseViewModelMixin<PartyPlayerNameplateVM>
    {
        public PlayerPartyNameplateVMMixin(PartyPlayerNameplateVM vm) : base(vm)
        {
            DiplomacyEvents.KingdomBannerChanged.AddNonSerializedListener(this, KingdomBannerChanged);
        }

        private void KingdomBannerChanged(Kingdom obj)
        {
            if (ViewModel!.Party is not null)
                ViewModel.RefreshValues();
        }

        public override void OnFinalize()
        {
            DiplomacyEvents.RemoveListeners(this);
        }
    }
}