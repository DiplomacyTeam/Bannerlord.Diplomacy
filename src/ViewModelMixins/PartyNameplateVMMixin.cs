using Bannerlord.UIExtenderEx.ViewModels;
using Diplomacy.Event;
using SandBox.ViewModelCollection.Nameplate;
using TaleWorlds.CampaignSystem;

namespace Diplomacy.ViewModelMixins
{
    class PartyNameplateVMMixin : BaseViewModelMixin<PartyNameplateVM>
    {
        public PartyNameplateVMMixin(PartyNameplateVM vm) : base(vm)
        {
            Events.KingdomBannerChanged.AddNonSerializedListener(this, this.KingdomBannerChanged);
        }

        private void KingdomBannerChanged(Kingdom obj)
        {
            ViewModel!.RefreshValues();
        }

        public override void OnFinalize()
        {
            Events.RemoveListeners(this);
        }
    }
}
