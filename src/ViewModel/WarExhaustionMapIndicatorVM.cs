using Diplomacy.Event;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Diplomacy.ViewModel
{
    internal class WarExhaustionMapIndicatorVM : TaleWorlds.Library.ViewModel
    {
        private MBBindingList<WarExhaustionMapIndicatorItemVM> _kingdomsAtWar;

        public WarExhaustionMapIndicatorVM()
        {
            this.RefreshValues();
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, HandleStanceChange);
            CampaignEvents.MakePeace.AddNonSerializedListener(this, HandleStanceChange);
            CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, (x, y, z, a, b) => HandleClanChangedKingdom(x));
#if e159
            CampaignEvents.MercenaryClanChangedKingdom.AddNonSerializedListener(this, (x, y, z) => HandleClanChangedKingdom(x));
#endif
            Events.WarExhaustionAdded.AddNonSerializedListener(this, HandleWarExhaustionChange);
        }

        private void HandleClanChangedKingdom(Clan clan)
        {
            if (Clan.PlayerClan == clan)
            {
                RefreshValues();
            }
        }

        private void HandleWarExhaustionChange(WarExhaustionEvent warExhaustionEvent)
        {
            Kingdom playerKingdom = Clan.PlayerClan.Kingdom;
            if (warExhaustionEvent.Kingdom == playerKingdom || warExhaustionEvent.OtherKingdom == playerKingdom)
            {
                foreach (WarExhaustionMapIndicatorItemVM item in _kingdomsAtWar)
                    item.UpdateWarExhaustion();
            }
        }

        private void HandleStanceChange(IFaction arg1, IFaction arg2)
        {
            Kingdom? kingdom1 = arg1 as Kingdom;
            Kingdom? kingdom2 = arg2 as Kingdom;

            Kingdom playerKingdom = Clan.PlayerClan.Kingdom;
            bool isEitherPlayerFaction = playerKingdom != null && (playerKingdom == kingdom1 || playerKingdom == kingdom2);

            if (kingdom1 != null && kingdom2 != null && isEitherPlayerFaction)
            {
                RefreshValues();
            }
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            CampaignEvents.WarDeclared.ClearListeners(this);
            CampaignEvents.MakePeace.ClearListeners(this);
            CampaignEvents.ClanChangedKingdom.ClearListeners(this);
            Events.WarExhaustionAdded.ClearListeners(this);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            KingdomsAtWar ??= new MBBindingList<WarExhaustionMapIndicatorItemVM>();
            KingdomsAtWar.Clear();

            foreach (Kingdom enemyKingdom in FactionManager.GetEnemyKingdoms(Clan.PlayerClan.Kingdom))
                KingdomsAtWar.Add(new WarExhaustionMapIndicatorItemVM(enemyKingdom));
        }

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
    }
}