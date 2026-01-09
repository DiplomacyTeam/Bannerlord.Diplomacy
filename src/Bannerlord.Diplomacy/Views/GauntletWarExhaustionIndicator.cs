using Diplomacy.ViewModel;

using JetBrains.Annotations;

using SandBox.View.Map;

using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace Diplomacy.Views
{
    [ViewCreatorModule]
    [UsedImplicitly]
    public class GauntletWarExhaustionIndicator : MapView
    {
        private WarExhaustionMapIndicatorVM? _dataSource = null!;
        private GauntletLayer? _layerAsGauntletLayer = null!;
        private GauntletMovieIdentifier? _movie;

        protected override void CreateLayout()
        {
            base.CreateLayout();
            _dataSource = new WarExhaustionMapIndicatorVM();
            _layerAsGauntletLayer = new GauntletLayer("WarExhaustionMapIndicatorLayer", 101);
            _movie = _layerAsGauntletLayer!.LoadMovie("WarExhaustionMapIndicator", _dataSource);
            Layer = _layerAsGauntletLayer;
            Layer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.MouseButtons | InputUsageMask.Keyboardkeys);
            MapScreen.AddLayer(Layer);
        }

        protected override void OnFinalize()
        {
            base.OnFinalize();
            _dataSource?.OnFinalize();
            _movie = null!;
        }

        protected override void OnMapConversationStart()
        {
            base.OnMapConversationStart();
            if (_layerAsGauntletLayer == null)
                return;
            ScreenManager.SetSuspendLayer(_layerAsGauntletLayer, true);
        }

        protected override void OnMapConversationOver()
        {
            base.OnMapConversationOver();
            if (_layerAsGauntletLayer == null)
                return;
            ScreenManager.SetSuspendLayer(_layerAsGauntletLayer, false);
        }
    }
}