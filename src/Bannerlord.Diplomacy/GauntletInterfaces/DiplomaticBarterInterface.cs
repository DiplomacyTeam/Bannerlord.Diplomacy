using Diplomacy.ViewModel;
using JetBrains.Annotations;
using SandBox.GauntletUI.Map;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;

namespace Diplomacy.GauntletInterfaces
{
    internal class DiplomaticBarterInterface : GenericInterface
    {
        protected override string MovieName => "DiplomaticBarter";

        public void ShowInterface(ScreenBase screenBase, Kingdom otherKingdom)
        {
            _screenBase = screenBase;

            var spriteData = UIResourceManager.SpriteData;
            var resourceContext = UIResourceManager.ResourceContext;
            var resourceDepot = UIResourceManager.UIResourceDepot;
            spriteData.SpriteCategories["ui_encyclopedia"].Load(resourceContext, resourceDepot);
            spriteData.SpriteCategories["ui_kingdom"].Load(resourceContext, resourceDepot);
            spriteData.SpriteCategories["ui_barter"].Load(resourceContext, resourceDepot);
            spriteData.SpriteCategories["ui_diplomacy"].Load(resourceContext, resourceDepot);

            _layer = new GauntletLayer(209);
            _layer.InputRestrictions.SetInputRestrictions();
            _layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
            _layer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_layer);
            screenBase.AddLayer(_layer);
            _vm = new DiplomaticBarterVM(Clan.PlayerClan.MapFaction, otherKingdom, OnFinalize);
            _movie = LoadMovie();

            if (GameStateManager.Current.ActiveState is MapState)
            {
                var mapBar = MapScreen.Instance.GetMapView<GauntletMapBar>();
                var method = new Reflect.Method(typeof(GauntletMapBar), "OnMapConversationStart").MethodInfo;
                method.Invoke(mapBar, new object[] { });
            }
            Campaign.Current.SetTimeSpeed(0);
        }

        [UsedImplicitly]
        protected override void OnFinalize()
        {
            base.OnFinalize();
            if (GameStateManager.Current.ActiveState is MapState)
            {
                var mapBar = MapScreen.Instance.GetMapView<GauntletMapBar>();
                var method = new Reflect.Method(typeof(GauntletMapBar), "OnMapConversationOver").MethodInfo;
                method.Invoke(mapBar, new object[] { });
            }
        }
    }
}