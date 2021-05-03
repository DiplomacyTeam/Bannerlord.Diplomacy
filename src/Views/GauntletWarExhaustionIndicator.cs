using Diplomacy.ViewModel;
using SandBox.View.Map;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;

namespace Diplomacy.Views
{
    [ViewCreatorModule]
    [OverrideView(typeof(MapWarExhaustionIndicatorView))]
    public class GauntletWarExhaustionIndicator : MapView
    {

        private WarExhaustionMapIndicatorVM _dataSource;
        private IGauntletMovie _movie;
        private GauntletLayer _layerAsGauntletLayer;

        protected override void CreateLayout()
        {
            base.CreateLayout();
            _dataSource = new WarExhaustionMapIndicatorVM();
            Layer = new GauntletLayer(100, "GauntletLayer");
            _layerAsGauntletLayer = (Layer as GauntletLayer)!;
            _movie = _layerAsGauntletLayer!.LoadMovie("WarExhaustionMapIndicator", _dataSource);
            Layer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.MouseButtons | InputUsageMask.Keyboardkeys);
            MapScreen.AddLayer(Layer);
        }
    }
}
