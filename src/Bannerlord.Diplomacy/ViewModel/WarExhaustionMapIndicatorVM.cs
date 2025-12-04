using Diplomacy.Events;
using Diplomacy.Extensions;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Diplomacy.ViewModel
{
    internal sealed class WarExhaustionMapIndicatorVM : TaleWorlds.Library.ViewModel
    {
        private MBBindingList<WarExhaustionMapIndicatorItemVM> _kingdomsAtWar;

        [DataSourceProperty]
        public MBBindingList<WarExhaustionMapIndicatorItemVM> KingdomsAtWar { get => _kingdomsAtWar; set => SetField(ref _kingdomsAtWar, value, nameof(KingdomsAtWar)); }

        public WarExhaustionMapIndicatorVM()
        {
            _kingdomsAtWar = new MBBindingList<WarExhaustionMapIndicatorItemVM>();
            RefreshValues();
            DiplomacyEvents.WarExhaustionInitialized.AddNonSerializedListener(this, HandleStanceChange);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, (x, _, _, _, _) => HandleClanChangedKingdom(x));
            DiplomacyEvents.WarExhaustionAdded.AddNonSerializedListener(this, HandleWarExhaustionChange);
            Settings.Instance!.PropertyChanged += Settings_PropertyChanged;
        }

        private void HandleClanChangedKingdom(Clan clan)
        {
            if (Clan.PlayerClan == clan) RefreshValues();
        }

        private void HandleWarExhaustionChange(WarExhaustionAddedEvent warExhaustionEvent)
        {
            var playerKingdom = Clan.PlayerClan.Kingdom;
            if (warExhaustionEvent.Kingdom == playerKingdom || warExhaustionEvent.OtherKingdom == playerKingdom)
                foreach (var item in _kingdomsAtWar)
                    item.UpdateWarExhaustion();
        }

        private void HandleStanceChange(WarExhaustionInitializedEvent warExhaustionEvent)
        {
            if (Clan.PlayerClan.MapFaction is Kingdom playerKingdom && (warExhaustionEvent.Kingdom == playerKingdom || warExhaustionEvent.OtherKingdom == playerKingdom))
                RefreshValues();
        }

        public override void OnFinalize()
        {
            base.OnFinalize();

            CampaignEvents.OnClanChangedKingdomEvent.ClearListeners(this);
            DiplomacyEvents.WarExhaustionInitialized.ClearListeners(this);
            DiplomacyEvents.WarExhaustionAdded.ClearListeners(this);
        }

        public void UpdateBanners()
        {
            foreach (var warExhaustionMapIndicatorItemVM in KingdomsAtWar)
            {
                warExhaustionMapIndicatorItemVM.RefreshValues();
            }
        }

        public override void RefreshValues()
        {
            KingdomsAtWar.Clear();
            if (!Settings.Instance!.EnableWarExhaustionCampaignMapWidget)
                return;

            if (Clan.PlayerClan.MapFaction is Kingdom playerKingdom)
                foreach (var enemyKingdom in KingdomExtensions.AllActiveKingdoms.Where(x => x.IsAtWarWith(playerKingdom) || x.IsAtConstantWarWith(playerKingdom)))
                    KingdomsAtWar.Add(new WarExhaustionMapIndicatorItemVM(enemyKingdom));
        }

        private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.Instance.EnableWarExhaustionCampaignMapWidget))
            {
                RefreshValues();
            }
        }
    }
}