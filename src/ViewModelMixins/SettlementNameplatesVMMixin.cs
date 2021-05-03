using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using Diplomacy.Event;
using SandBox.ViewModelCollection.Nameplate;
using TaleWorlds.CampaignSystem;

namespace Diplomacy.ViewModelMixins
{
    [ViewModelMixin]
    public class SettlementNameplatesVMMixin : BaseViewModelMixin<SettlementNameplatesVM>
    {
        public SettlementNameplatesVMMixin(SettlementNameplatesVM vm) : base(vm)
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
