using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using Diplomacy.GauntletInterfaces;

using JetBrains.Annotations;

using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;

namespace Diplomacy.ViewModelMixin
{
    [ViewModelMixin]
    [UsedImplicitly]
    internal sealed class KingdomManagementVMMixin : BaseViewModelMixin<KingdomManagementVM>
    {
        [DataSourceProperty]
        public string FactionsLabel { get; set; }

        public KingdomManagementVMMixin(KingdomManagementVM vm) : base(vm)
        {
            FactionsLabel = new TextObject("{=gypPPxUJ}Factions").ToString();
        }

        public override void OnFinalize()
        {
            ViewModel!.Diplomacy.OnFinalize();
        }

        [DataSourceMethod]
        [UsedImplicitly]
        public void ExecuteShowFactions()
        {
            new RebelFactionsInterface().ShowInterface(ScreenManager.TopScreen, ViewModel!.Kingdom);
        }
    }
}