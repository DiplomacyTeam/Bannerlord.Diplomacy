using Diplomacy.ViewModel;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.ScreenSystem;

namespace Diplomacy.GauntletInterfaces
{
    internal sealed class GrantFiefInterface : GenericInterface
    {
        private Action? _refreshAction;

        protected override string MovieName => "GrantFief";

        public void ShowFiefInterface(ScreenBase screenBase, Hero hero, Action refreshAction)
        {
            if (!ShowInterfaceWithCheck())
                return;

            _screenBase = screenBase;
            _refreshAction = refreshAction;

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
            _vm = new GrantFiefVM(hero, OnFinalize);
            _movie = LoadMovie();
        }

        public void ShowFiefInterface(ScreenBase screenBase, Hero hero)
        {
            ShowFiefInterface(screenBase, hero, () => { });
        }

        protected override void OnFinalize()
        {
            base.OnFinalize();
            _refreshAction!();
        }
    }
}