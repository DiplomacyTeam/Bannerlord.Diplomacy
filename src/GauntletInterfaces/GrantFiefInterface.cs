using Diplomacy.ViewModel;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace Diplomacy.GauntletInterfaces
{
    class GrantFiefInterface
    {
#if STABLE
        private GauntletMovie _movie;
#else
        private IGauntletMovie? _movie;
#endif
        private GauntletLayer? _layer;
        private GrantFiefVM? _vm;
        private ScreenBase? _screenBase;

        public void ShowFiefInterface(ScreenBase screenBase, Hero hero, Action refreshAction)
        {
            _screenBase = screenBase;

            var spriteData = UIResourceManager.SpriteData;
            var resourceContext = UIResourceManager.ResourceContext;
            var resourceDepot = UIResourceManager.UIResourceDepot;
            spriteData.SpriteCategories["ui_encyclopedia"].Load(resourceContext, resourceDepot);
            spriteData.SpriteCategories["ui_kingdom"].Load(resourceContext, resourceDepot);

            _layer = new GauntletLayer(211);
            _layer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
            _layer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_layer);
            screenBase.AddLayer(_layer);
            _vm = new GrantFiefVM(hero, () => OnFinalize(refreshAction));
            _movie = _layer.LoadMovie("GrantFief", _vm);
        }

        public void ShowFiefInterface(ScreenBase screenBase, Hero hero)
        {
            ShowFiefInterface(screenBase, hero, () => { });
        }

        public void OnFinalize(Action action)
        {
            _screenBase?.RemoveLayer(_layer);
            _layer?.ReleaseMovie(_movie);
            _layer = null;
            _movie = null;
            // vm.ExecuteSelect(null);
            // vm.AssignParent(true);
            _vm = null;
            _screenBase = null;
            action();
        }
    }
}
