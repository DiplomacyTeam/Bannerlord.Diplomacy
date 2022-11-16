using Diplomacy.Views;

using SandBox.View.Map;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class UIBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.TickEvent.AddNonSerializedListener(this, AddUIElements);
        }

        private void AddUIElements(float obj)
        {
            if (!Settings.Instance!.EnableWarExhaustion)
            {
                CampaignEvents.TickEvent.ClearListeners(this);
            }
            else if (Game.Current.GameStateManager.ActiveState is MapState)
            {
                MapScreen.Instance.AddMapView<MapWarExhaustionIndicatorView>();
                CampaignEvents.TickEvent.ClearListeners(this);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}