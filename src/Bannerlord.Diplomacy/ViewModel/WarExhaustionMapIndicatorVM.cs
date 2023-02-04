using Diplomacy.Events;

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
#if v100 || v101 || v102 || v103
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, HandleStanceChange);
            CampaignEvents.MakePeace.AddNonSerializedListener(this, HandleStanceChange);
#else
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, (arg1, arg2, _) => HandleStanceChange(arg1, arg2));
            CampaignEvents.MakePeace.AddNonSerializedListener(this, (arg1, arg2, _) => HandleStanceChange(arg1, arg2));
#endif
            CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, (x, _, _, _, _) => HandleClanChangedKingdom(x));
            DiplomacyEvents.WarExhaustionAdded.AddNonSerializedListener(this, HandleWarExhaustionChange);
            Settings.Instance!.PropertyChanged += Settings_PropertyChanged;
        }

        private void HandleClanChangedKingdom(Clan clan)
        {
            if (Clan.PlayerClan == clan) RefreshValues();
        }

        private void HandleWarExhaustionChange(WarExhaustionEvent warExhaustionEvent)
        {
            var playerKingdom = Clan.PlayerClan.Kingdom;
            if (warExhaustionEvent.Kingdom == playerKingdom || warExhaustionEvent.OtherKingdom == playerKingdom)
                foreach (var item in _kingdomsAtWar)
                    item.UpdateWarExhaustion();
        }

        private void HandleStanceChange(IFaction arg1, IFaction arg2)
        {
            if (Clan.PlayerClan.MapFaction is Kingdom playerKingdom
                && arg1 is Kingdom kingdom1
                && arg2 is Kingdom kingdom2
                && (kingdom1 == playerKingdom || kingdom2 == playerKingdom))
                RefreshValues();
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            CampaignEvents.WarDeclared.ClearListeners(this);
            CampaignEvents.MakePeace.ClearListeners(this);
            CampaignEvents.ClanChangedKingdom.ClearListeners(this);
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