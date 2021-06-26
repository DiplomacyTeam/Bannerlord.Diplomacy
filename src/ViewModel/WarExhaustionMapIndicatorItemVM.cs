using Diplomacy.GauntletInterfaces;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;

namespace Diplomacy.ViewModel
{
    class WarExhaustionMapIndicatorItemVM : TaleWorlds.Library.ViewModel
    {
        private readonly Kingdom _opposingKingdom;
        private int _playerWarExhaustion, _opponentWarExhaustion;
        private ImageIdentifierVM _faction1Visual;
        private ImageIdentifierVM _faction2Visual;
        private bool _isCriticalFaction1;
        private bool _isCriticalFaction2;

        public WarExhaustionMapIndicatorItemVM(Kingdom opposingKingdom)
        {
            this._opposingKingdom = opposingKingdom;
            this.RefreshValues();
        }
        public override void RefreshValues()
        {
            base.RefreshValues();
            var playerKingdom = Clan.PlayerClan.Kingdom;

            UpdateWarExhaustion();
            Faction1Visual = new ImageIdentifierVM(BannerCode.CreateFrom(playerKingdom.Banner), true);
            Faction2Visual = new ImageIdentifierVM(BannerCode.CreateFrom(_opposingKingdom.Banner), true);
        }

        public void UpdateWarExhaustion()
        {
            PlayerWarExhaustion = (int)WarExhaustionManager.Instance.GetWarExhaustion(Clan.PlayerClan.Kingdom, _opposingKingdom);
            OpponentWarExhaustion = (int)WarExhaustionManager.Instance.GetWarExhaustion(_opposingKingdom, Clan.PlayerClan.Kingdom);
            IsCriticalFaction1 = (PlayerWarExhaustion / WarExhaustionManager.MaxWarExhaustion) >= 0.75f;
            IsCriticalFaction2 = (OpponentWarExhaustion / WarExhaustionManager.MaxWarExhaustion) >= 0.75f;
        }

        private void OpenDetailedWarView()
        {
            new DetailWarViewInterface().ShowInterface(ScreenManager.TopScreen, _opposingKingdom);
        }

        [DataSourceProperty]
        public int OpponentWarExhaustion
        {
            get => _opponentWarExhaustion;
            set
            {
                if (value != _opponentWarExhaustion)
                {
                    _opponentWarExhaustion = value;
                    OnPropertyChanged(nameof(OpponentWarExhaustion));
                }
            }
        }

        [DataSourceProperty]
        public int PlayerWarExhaustion
        {
            get => _playerWarExhaustion;
            set
            {
                if (value != _playerWarExhaustion)
                {
                    _playerWarExhaustion = value;
                    OnPropertyChanged(nameof(PlayerWarExhaustion));
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM Faction1Visual
        {
            get
            {
                return this._faction1Visual;
            }
            set
            {
                if (value != this._faction1Visual)
                {
                    this._faction1Visual = value;
                    base.OnPropertyChangedWithValue(value, "Faction1Visual");
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM Faction2Visual
        {
            get
            {
                return this._faction2Visual;
            }
            set
            {
                if (value != this._faction2Visual)
                {
                    this._faction2Visual = value;
                    base.OnPropertyChangedWithValue(value, "Faction2Visual");
                }
            }
        }

        [DataSourceProperty]
        public bool IsCriticalFaction1
        {
            get => _isCriticalFaction1;
            set
            {
                if (value != this._isCriticalFaction1)
                {
                    this._isCriticalFaction1 = value;
                    base.OnPropertyChangedWithValue(value, nameof(IsCriticalFaction1));
                }
            }
        }

        [DataSourceProperty]
        public bool IsCriticalFaction2
        {
            get => _isCriticalFaction2;
            set
            {
                if (value != this._isCriticalFaction2)
                {
                    this._isCriticalFaction2 = value;
                    base.OnPropertyChangedWithValue(value, nameof(IsCriticalFaction2));
                }
            }
        }

    }
}
