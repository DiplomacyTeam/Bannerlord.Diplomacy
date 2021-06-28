using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using Diplomacy.GauntletInterfaces;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModelMixins
{
    [ViewModelMixin]
    internal sealed class KingdomManagementVMMixin : BaseViewModelMixin<KingdomManagementVM>
    {
        [DataSourceProperty]
        public string FactionsLabel { get; set; }

        public KingdomManagementVMMixin(KingdomManagementVM vm) : base(vm)
        {
            FactionsLabel = new TextObject("{=gypPPxUJ}Factions").ToString();
        }

#if !(e159 || e1510)
        public override void OnFinalize()
        {
            ViewModel!.Diplomacy.OnFinalize();
        }
#endif

        [DataSourceMethod]
        public void ExecuteShowFactions()
        {
            new RebelFactionsInterface().ShowInterface(ScreenManager.TopScreen, ViewModel!.Kingdom);
        }
    }
}
