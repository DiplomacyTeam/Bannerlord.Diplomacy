using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using Diplomacy.Extensions;

using JetBrains.Annotations;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapBar;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModelMixin
{
    [ViewModelMixin("RefreshPermissionValues")]
    [UsedImplicitly]
    internal sealed class MapNavigationVMMixin : BaseViewModelMixin<MapNavigationVM>
    {
        private static readonly TextObject _TRebelKingdomText = new("{=f66sNaiz}You cannot be part of a rebellion");

        public MapNavigationVMMixin(MapNavigationVM vm) : base(vm) { }

        public override void OnRefresh()
        {
            if (Clan.PlayerClan.Kingdom?.IsRebelKingdom() ?? false)
            {
                ViewModel!.IsKingdomEnabled = false;
                ViewModel!.KingdomHint.SetHintCallback(() => _TRebelKingdomText.ToString());
            }
        }
    }
}