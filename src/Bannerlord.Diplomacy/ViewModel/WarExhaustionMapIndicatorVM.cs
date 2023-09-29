using Diplomacy.Events;

using System;

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
#if v124
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, (x, _, _, _, _) => HandleClanChangedKingdom(x));
#elif v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115 || v116 || v120 || v121 || v122 || v123
            CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, (x, _, _, _, _) => HandleClanChangedKingdom(x));
#endif
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

#if v124
            CampaignEvents.OnClanChangedKingdomEvent.ClearListeners(this);
#elif v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115 || v116 || v120 || v121 || v122 || v123
            CampaignEvents.ClanChangedKingdom.ClearListeners(this);
#endif
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
                foreach (var enemyKingdom in FactionManager.GetEnemyKingdoms(playerKingdom))
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