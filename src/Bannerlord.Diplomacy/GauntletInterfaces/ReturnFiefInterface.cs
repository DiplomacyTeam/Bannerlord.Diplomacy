using Diplomacy.Actions;
using Diplomacy.ViewModel;

using System;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.ScreenSystem;

namespace Diplomacy.GauntletInterfaces
{
    /// <summary>
    /// Reuses the GrantFief movie: <see cref="ReturnFiefVM"/> exposes the same bindings, so the
    /// only thing that differs is the data behind them.
    /// </summary>
    internal sealed class ReturnFiefInterface : GenericInterface
    {
        private Action? _refreshAction;

        protected override string MovieName => "GrantFief";

        public void ShowReturnFiefInterface(ScreenBase screenBase, Kingdom targetKingdom, Action refreshAction)
        {
            // The button gates on this too, but a stale refresh must not reach an empty picker.
            if (!ReturnFiefAction.GetReturnableFiefs(targetKingdom).Any())
                return;

            if (!ShowInterfaceWithCheck())
                return;

            _screenBase = screenBase;
            _refreshAction = refreshAction;

            var spriteData = UIResourceManager.SpriteData;
            var resourceContext = UIResourceManager.ResourceContext;
            var resourceDepot = UIResourceManager.ResourceDepot;
            spriteData.SpriteCategories["ui_encyclopedia"].Load(resourceContext, resourceDepot);
            spriteData.SpriteCategories["ui_kingdom"].Load(resourceContext, resourceDepot);

            _layer = new GauntletLayer("ReturnFiefLayer", 211);
            _layer.InputRestrictions.SetInputRestrictions();
            _layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
            _layer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_layer);
            screenBase.AddLayer(_layer);
            _vm = new ReturnFiefVM(targetKingdom, OnFinalize);
            _movie = LoadMovie();
        }

        protected override void OnFinalize()
        {
            base.OnFinalize();
            _refreshAction!();
        }
    }
}
