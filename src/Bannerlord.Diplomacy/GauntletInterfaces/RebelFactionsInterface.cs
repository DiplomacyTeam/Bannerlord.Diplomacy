using Diplomacy.ViewModel;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.ScreenSystem;

namespace Diplomacy.GauntletInterfaces
{
    internal sealed class RebelFactionsInterface : GenericInterface
    {
        protected override string MovieName => "RebelFactions";

        public void ShowInterface(ScreenBase screenBase, Kingdom kingdom)
        {
            if (!ShowInterfaceWithCheck())
                return;

            _screenBase = screenBase;

            var spriteData = UIResourceManager.SpriteData;
            var resourceContext = UIResourceManager.ResourceContext;
            var resourceDepot = UIResourceManager.UIResourceDepot;
            spriteData.SpriteCategories["ui_encyclopedia"].Load(resourceContext, resourceDepot);
            spriteData.SpriteCategories["ui_kingdom"].Load(resourceContext, resourceDepot);

            _layer = new GauntletLayer(211);
            _layer.InputRestrictions.SetInputRestrictions();
            _layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
            _layer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_layer);
            screenBase.AddLayer(_layer);
            _vm = new RebelFactionsVM(kingdom, OnFinalize);
            _movie = LoadMovie();
        }
    }
}