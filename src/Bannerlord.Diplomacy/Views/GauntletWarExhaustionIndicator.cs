using Diplomacy.ViewModel;

using JetBrains.Annotations;

using SandBox.View.Map;

using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;

namespace Diplomacy.Views
{
    [ViewCreatorModule]
    [OverrideView(typeof(MapWarExhaustionIndicatorView))]
    [UsedImplicitly]
    public class GauntletWarExhaustionIndicator : MapView
    {
        private WarExhaustionMapIndicatorVM _dataSource = null!;
        private GauntletLayer _layerAsGauntletLayer = null!;

        protected override void CreateLayout()
        {
            base.CreateLayout();
            _dataSource = new WarExhaustionMapIndicatorVM();
            Layer = new GauntletLayer(100);
            _layerAsGauntletLayer = (Layer as GauntletLayer)!;
            _layerAsGauntletLayer!.LoadMovie("WarExhaustionMapIndicator", _dataSource);
            Layer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.MouseButtons | InputUsageMask.Keyboardkeys);
            MapScreen.AddLayer(Layer);
        }
    }
}