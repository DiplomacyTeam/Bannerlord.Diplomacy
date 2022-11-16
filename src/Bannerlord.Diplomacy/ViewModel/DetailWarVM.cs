using JetBrains.Annotations;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

using static Diplomacy.WarExhaustionManager;

namespace Diplomacy.ViewModel
{
    internal sealed class DetailWarVM : TaleWorlds.Library.ViewModel
    {
        private readonly Action _finalize;
        private readonly Kingdom _opposingKingdom;
        private string _opponentWarExhaustion = null!;
        private string _playerWarExhaustion = null!;

        [DataSourceProperty] public string StartDate { get; set; }

        [DataSourceProperty] public string Duration { get; set; }

        [DataSourceProperty] public string WarExhaustionLabel { get; set; }

        [DataSourceProperty] public string WarExhaustionReportLabel { get; set; }

        [DataSourceProperty] public string KingdomName { get; set; }

        [DataSourceProperty] public string OpponentKingdomName { get; set; }

        [DataSourceProperty] public HeroVM FactionLeader { get; set; }

        [DataSourceProperty] public HeroVM OpponentFactionLeader { get; set; }

        [DataSourceProperty] public DiplomacyFactionRelationshipVM OpponentKingdom { get; set; }

        [DataSourceProperty] public DiplomacyFactionRelationshipVM Kingdom { get; set; }

        [DataSourceProperty] public DetailWarStatsVM Stats { get; set; } = null!;

        [DataSourceProperty] public string StatsLabel { get; set; }

        [DataSourceProperty] public DetailWarStatsVM OpponentStats { get; set; } = null!;

        [DataSourceProperty] public HintViewModel HelpHint { get; set; }

        [DataSourceProperty] public string WarExhaustionRate { get; set; }

        [DataSourceProperty] public HintViewModel RateHelpHint { get; set; }

        [DataSourceProperty] public string WarReportLabel { get; set; }

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

        [DataSourceProperty] public ImageIdentifierVM Faction1Visual { get; set; } = null!;

        [DataSourceProperty] public ImageIdentifierVM Faction2Visual { get; set; } = null!;

        [DataSourceProperty] public MBBindingList<WarExhaustionBreakdownVM> Breakdowns { get; set; }

        public DetailWarVM(Kingdom opposingKingdom, Action onFinalize)
        {
            _opposingKingdom = opposingKingdom;
            _finalize = onFinalize;
            Breakdowns = new MBBindingList<WarExhaustionBreakdownVM>();
            var warStartDate = Clan.PlayerClan.Kingdom.GetStanceWith(_opposingKingdom).WarStartDate;
            StartDate = new TextObject("{=hAoIC2Iq}Date Started: {START_DATE}")
                .SetTextVariable("START_DATE", warStartDate.ToString())
                .ToString();
            Duration = new TextObject("{=qHrihV27}War Duration: {WAR_DURATION} days")
                .SetTextVariable("WAR_DURATION", (int) warStartDate.ElapsedDaysUntilNow)
                .ToString();
            WarExhaustionLabel = GameTexts.FindText("str_war_exhaustion").ToString() + "!!!";
            WarExhaustionReportLabel = new TextObject("{=IlFEeNkY}War Exhaustion Report").ToString();
            KingdomName = Clan.PlayerClan.Kingdom.Name.ToString();
            OpponentKingdomName = _opposingKingdom.Name.ToString();
            FactionLeader = new HeroVM(Clan.PlayerClan.Kingdom.Leader);
            OpponentFactionLeader = new HeroVM(_opposingKingdom.Leader);
            OpponentKingdom = new DiplomacyFactionRelationshipVM(_opposingKingdom);
            Kingdom = new DiplomacyFactionRelationshipVM(Clan.PlayerClan.Kingdom);
            HelpHint = new HintViewModel(GameTexts.FindText("str_wardetail_help"));
            WarExhaustionRate = $"{Instance.GetWarExhaustionRate(Clan.PlayerClan.Kingdom, _opposingKingdom):p0}";
            RateHelpHint = new HintViewModel(GameTexts.FindText("str_warexhaustionrate_help"));
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

            PlayerWarExhaustion = $"{Instance.GetWarExhaustion(Clan.PlayerClan.Kingdom, _opposingKingdom):F1}%";
            OpponentWarExhaustion = $"{Instance.GetWarExhaustion(_opposingKingdom, Clan.PlayerClan.Kingdom):F1}%";

            Stats = new DetailWarStatsVM(playerKingdom);
            OpponentStats = new DetailWarStatsVM(_opposingKingdom);

            var breakdowns = Instance.GetWarExhaustionBreakdown(playerKingdom, _opposingKingdom);

            Breakdowns.Clear();
            foreach (var breakdown in breakdowns) Breakdowns.Add(new WarExhaustionBreakdownVM(breakdown));
        }

        [UsedImplicitly]
        private void OnComplete()
        {
            _finalize();
        }
    }
}