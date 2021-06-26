using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Party;
using static Diplomacy.WarExhaustionManager;

namespace Diplomacy.ViewModel
{
    internal class DetailWarVM : TaleWorlds.Library.ViewModel
    {
        private Kingdom _opposingKingdom;
        private Action _finalize;
        private ImageIdentifierVM _faction1Visual;
        private ImageIdentifierVM _faction2Visual;
        private string _opponentWarExhaustion;
        private string _playerWarExhaustion;

        [DataSourceProperty]
        public string StartDate { get; set; }
        [DataSourceProperty]
        public string Duration { get; set; }
        [DataSourceProperty]
        public string WarExhaustionLabel { get; set; }
        [DataSourceProperty]
        public string WarExhaustionReportLabel { get; set; }
        [DataSourceProperty]
        public string KingdomName { get; set; }
        [DataSourceProperty]
        public string OpponentKingdomName { get; set; }

        [DataSourceProperty]
        public HeroVM FactionLeader { get; set; }
        [DataSourceProperty]
        public HeroVM OpponentFactionLeader { get; set; }

        [DataSourceProperty]
        public DiplomacyFactionRelationshipVM OpponentKingdom { get; set; }
        [DataSourceProperty]
        public DiplomacyFactionRelationshipVM Kingdom { get; set; }
        [DataSourceProperty]
        public DetailWarStatsVM Stats { get; set; }
        [DataSourceProperty]
        public string StatsLabel { get; set; }

        [DataSourceProperty]
        public DetailWarStatsVM OpponentStats { get; set; }
        [DataSourceProperty]
        public HintViewModel HelpHint { get; set; }

        [DataSourceProperty]
        public string WarExhaustionRate { get; set; }
        [DataSourceProperty]
        public HintViewModel RateHelpHint { get; set; }
        [DataSourceProperty]
        public string WarReportLabel { get; set; }



        public DetailWarVM(Kingdom opposingKingdom, Action onFinalize)
        {
            _opposingKingdom = opposingKingdom;
            _finalize = onFinalize;
            Breakdowns = new();
            CampaignTime warStartDate = Clan.PlayerClan.Kingdom.GetStanceWith(_opposingKingdom).WarStartDate;
            StartDate = new TextObject("{=hAoIC2Iq}Date Started: {START_DATE}")
                .SetTextVariable("START_DATE", warStartDate.ToString())
                .ToString();
            Duration = new TextObject("{=qHrihV27}War Duration: {WAR_DURATION} days")
                .SetTextVariable("WAR_DURATION", (int)warStartDate.ElapsedDaysUntilNow)
                .ToString();
            WarExhaustionLabel = GameTexts.FindText("str_war_exhaustion").ToString();
            WarExhaustionReportLabel = new TextObject("{=IlFEeNkY}War Exhaustion Report").ToString();
            KingdomName = Clan.PlayerClan.Kingdom.Name.ToString();
            OpponentKingdomName = _opposingKingdom.Name.ToString();
            FactionLeader = new HeroVM(Clan.PlayerClan.Kingdom.Leader);
            OpponentFactionLeader = new HeroVM(_opposingKingdom.Leader);
            OpponentKingdom = new DiplomacyFactionRelationshipVM(_opposingKingdom);
            Kingdom = new DiplomacyFactionRelationshipVM(Clan.PlayerClan.Kingdom);
            HelpHint = new(GameTexts.FindText("str_wardetail_help"));
            WarExhaustionRate = string.Format("{0:p0}", WarExhaustionManager.Instance.GetWarExhaustionRate(Clan.PlayerClan.Kingdom, _opposingKingdom));
            RateHelpHint = new(GameTexts.FindText("str_warexhaustionrate_help"));
            StatsLabel = GameTexts.FindText("str_stat").ToString();
            WarReportLabel = new TextObject("{=mCue7aFc}War Report").ToString();
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            var playerKingdom = Clan.PlayerClan.Kingdom;

            Faction1Visual = new ImageIdentifierVM(BannerCode.CreateFrom(playerKingdom.Banner), true);
            Faction2Visual = new ImageIdentifierVM(BannerCode.CreateFrom(_opposingKingdom.Banner), true);
           
            PlayerWarExhaustion = string.Format("{0:F1}{1}", WarExhaustionManager.Instance.GetWarExhaustion(Clan.PlayerClan.Kingdom, _opposingKingdom), "%");
            OpponentWarExhaustion = string.Format("{0:F1}{1}", WarExhaustionManager.Instance.GetWarExhaustion(_opposingKingdom, Clan.PlayerClan.Kingdom), "%");

            Stats = new DetailWarStatsVM(playerKingdom);
            OpponentStats = new DetailWarStatsVM(_opposingKingdom);

            var breakdowns = WarExhaustionManager.Instance.GetWarExhaustionBreakdown(playerKingdom, _opposingKingdom);

            Breakdowns.Clear();
            foreach (WarExhaustionBreakdown breakdown in breakdowns) 
            {
                Breakdowns.Add(new WarExhaustionBreakdownVM(breakdown));
            }
        }

        private void OnComplete()
        {
            _finalize();
        }

        [DataSourceProperty]
        public string OpponentWarExhaustion
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
        public string PlayerWarExhaustion
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
        public MBBindingList<WarExhaustionBreakdownVM> Breakdowns { get; set; }
    }
}
