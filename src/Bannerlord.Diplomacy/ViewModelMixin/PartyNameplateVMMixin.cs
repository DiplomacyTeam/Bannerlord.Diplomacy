using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using Diplomacy.Event;

using JetBrains.Annotations;

using SandBox.ViewModelCollection.Nameplate;

using TaleWorlds.CampaignSystem;

namespace Diplomacy.ViewModelMixin
{
    [ViewModelMixin]
    [UsedImplicitly]
    public class PartyNameplateVMMixin : BaseViewModelMixin<PartyNameplateVM>
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