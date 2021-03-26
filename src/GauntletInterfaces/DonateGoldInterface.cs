using Diplomacy.ViewModel;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace Diplomacy.GauntletInterfaces
{
    class DonateGoldInterface
    {
        private GauntletLayer _layer;
        private IGauntletMovie _movie;
        private DonateGoldVM _vm;
        private ScreenBase _screenBase;

        public void ShowInterface(ScreenBase screenBase, Clan clan)
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
            _vm = new DonateGoldVM(clan, OnFinalize);
            _movie = _layer.LoadMovie("DonateGold", _vm);
        }

        public void OnFinalize()
        {
            _screenBase.RemoveLayer(_layer);
            _layer.ReleaseMovie(_movie);
            _layer = null;
            _movie = null;
            // vm.ExecuteSelect(null);
            // vm.AssignParent(true);
            _vm = null;
            _screenBase = null;
        }

    }
}
