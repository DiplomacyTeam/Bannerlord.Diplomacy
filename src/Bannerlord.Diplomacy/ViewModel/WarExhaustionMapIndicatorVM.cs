using Diplomacy.Event;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Diplomacy.ViewModel
{
    internal sealed class WarExhaustionMapIndicatorVM : TaleWorlds.Library.ViewModel
    {
        private MBBindingList<WarExhaustionMapIndicatorItemVM> _kingdomsAtWar;

        [DataSourceProperty]
        public MBBindingList<WarExhaustionMapIndicatorItemVM> KingdomsAtWar
        {
            get => _kingdomsAtWar;
            set
            {
                if (value != _kingdomsAtWar)
                {
                    _kingdomsAtWar = value;
                    OnPropertyChanged(nameof(KingdomsAtWar));
                }
            }
        }

        public WarExhaustionMapIndicatorVM()
        {
            _kingdomsAtWar = new MBBindingList<WarExhaustionMapIndicatorItemVM>();
            RefreshValues();
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, HandleStanceChange);
            CampaignEvents.MakePeace.AddNonSerializedListener(this, HandleStanceChange);
            CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, (x, _, _, _, _) => HandleClanChangedKingdom(x));
#if e159 || e1510
            CampaignEvents.MercenaryClanChangedKingdom.AddNonSerializedListener(this, (x, y, z) => HandleClanChangedKingdom(x));
#endif
            Events.WarExhaustionAdded.AddNonSerializedListener(this, HandleWarExhaustionChange);
        }

        private void HandleClanChangedKingdom(Clan clan)
        {
            if (Clan.PlayerClan == clan) RefreshValues();
        }

        private void HandleWarExhaustionChange(WarExhaustionEvent warExhaustionEvent)
        {
            Kingdom playerKingdom = Clan.PlayerClan.Kingdom;
            if (warExhaustionEvent.Kingdom == playerKingdom || warExhaustionEvent.OtherKingdom == playerKingdom)
                foreach (WarExhaustionMapIndicatorItemVM item in _kingdomsAtWar)
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
            Events.WarExhaustionAdded.ClearListeners(this);
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

            if (Clan.PlayerClan.MapFaction is Kingdom playerKingdom)
                foreach (Kingdom enemyKingdom in FactionManager.GetEnemyKingdoms(playerKingdom))
                    KingdomsAtWar.Add(new WarExhaustionMapIndicatorItemVM(enemyKingdom));
        }
    }
}