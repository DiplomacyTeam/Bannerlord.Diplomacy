using Diplomacy.Extensions;

using JetBrains.Annotations;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

using static Diplomacy.WarExhaustion.WarExhaustionManager;
using static Diplomacy.WarExhaustion.WarExhaustionRecord;

namespace Diplomacy.ViewModel
{
    internal sealed class DetailWarVM : TaleWorlds.Library.ViewModel
    {
        private const string _criticaExhaustionlHintText = "{=hfa8LLqD}{EXHAUSTED_FACTION} is critically depleted by war with {OTHER_FACTION}. Their ability to continue this war is at the limit, {?IS_CIVIL_WAR}which more and more confirms their understanding of the inevitability of making peace{?}making them more and more open to the possibility of peace{\\?} as war exhaustion nears its maximum.";
        private static TextObject _activeQuestWarning = new("{=k0Dnf6cU}Tied to an active quest!");
        private static TextObject _activeQuestHint = new("{=xPHDtsC1}There is an active quest that prevents this war from ending prematurely. To ensure stability, war exhaustion will under no circumstances exceed {CRITICAL_THRESHOLD}% for either side.");
        private static TextObject _hadActiveQuestWarning = new("{=xoY6cYDK}Was tied to an active quest!");
        private static TextObject _hadActiveQuestHint = new("{=UvjMFpHs}There was an active quest that kept this war from ending prematurely. As a consequence, the breakdown figures in the report may not match the overall war exhaustion values.");

        private readonly Action _finalize;
        private readonly Kingdom _opposingKingdom;
        private string _opponentWarExhaustion = null!;
        private string _playerWarExhaustion = null!;
        private bool _opponentWarExhaustionIsCritical;
        private bool _playerWarExhaustionIsCritical;

        [DataSourceProperty]
        public string StartDate { get; set; }

        [DataSourceProperty]
        public string Duration { get; set; }

        [DataSourceProperty]
        public string ActiveQuestWarning { get; set; }

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
        public DetailWarStatsVM Stats { get; set; } = null!;

        [DataSourceProperty]
        public string StatsLabel { get; set; }

        [DataSourceProperty]
        public DetailWarStatsVM OpponentStats { get; set; } = null!;

        [DataSourceProperty]
        public HintViewModel HelpHint { get; set; }

        [DataSourceProperty]
        public string WarExhaustionRate { get; set; }

        [DataSourceProperty]
        public HintViewModel RateHelpHint { get; set; }

        [DataSourceProperty]
        public HintViewModel ActiveQuestHint { get; set; }

        [DataSourceProperty]
        public string WarReportLabel { get; set; }

        [DataSourceProperty]
        public string OpponentWarExhaustion { get => _opponentWarExhaustion; set => SetField(ref _opponentWarExhaustion, value, nameof(OpponentWarExhaustion)); }

        [DataSourceProperty]
        public bool OpponentWarExhaustionIsCritical { get => _opponentWarExhaustionIsCritical; set => SetField(ref _opponentWarExhaustionIsCritical, value, nameof(OpponentWarExhaustionIsCritical)); }

        [DataSourceProperty]
        public HintViewModel OpponentCriticaExhaustionlHint { get; set; }

        [DataSourceProperty]
        public string PlayerWarExhaustion { get => _playerWarExhaustion; set => SetField(ref _playerWarExhaustion, value, nameof(PlayerWarExhaustion)); }

        [DataSourceProperty]
        public bool PlayerWarExhaustionIsCritical { get => _playerWarExhaustionIsCritical; set => SetField(ref _playerWarExhaustionIsCritical, value, nameof(PlayerWarExhaustionIsCritical)); }

        [DataSourceProperty]
        public HintViewModel PlayerCriticaExhaustionlHint { get; set; }

        [DataSourceProperty]
        public ImageIdentifierVM Faction1Visual { get; set; } = null!;

        [DataSourceProperty]
        public ImageIdentifierVM Faction2Visual { get; set; } = null!;

        [DataSourceProperty]
        public MBBindingList<WarExhaustionBreakdownVM> Breakdowns { get; set; }

        [DataSourceProperty]
        public int ReportHeight { get; set; }

        [DataSourceProperty]
        public int CloseButtonYOffset => -ReportHeight / 2;

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
            WarExhaustionLabel = GameTexts.FindText("str_war_exhaustion").ToString();
            WarExhaustionReportLabel = new TextObject("{=IlFEeNkY}War Exhaustion Report").ToString();
            KingdomName = Clan.PlayerClan.Kingdom.Name.ToString();
            OpponentKingdomName = _opposingKingdom.Name.ToString();
            FactionLeader = new HeroVM(Clan.PlayerClan.Kingdom.Leader);
            OpponentFactionLeader = new HeroVM(_opposingKingdom.Leader);
            OpponentKingdom = new DiplomacyFactionRelationshipVM(_opposingKingdom);
            Kingdom = new DiplomacyFactionRelationshipVM(Clan.PlayerClan.Kingdom);
            HelpHint = new HintViewModel(GameTexts.FindText("str_wardetail_help"));
            WarExhaustionRate = Settings.Instance!.IndividualWarExhaustionRates ? $"{Instance!.GetWarExhaustionRate(Clan.PlayerClan.Kingdom, _opposingKingdom):0%} / {Instance.GetWarExhaustionRate(_opposingKingdom, Clan.PlayerClan.Kingdom):0%}"
                                                                                : $"{Instance!.GetWarExhaustionRate(Clan.PlayerClan.Kingdom, _opposingKingdom):0%}";
            RateHelpHint = new HintViewModel(GameTexts.FindText("str_warexhaustionrate_help"));
            StatsLabel = GameTexts.FindText("str_stat").ToString();
            WarReportLabel = new TextObject("{=mCue7aFc}War Report").ToString();
            ActiveQuestWarning = "";
            ActiveQuestHint = new(TextObject.Empty);
            PlayerCriticaExhaustionlHint = new(TextObject.Empty);
            OpponentCriticaExhaustionlHint = new(TextObject.Empty);

            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            var playerKingdom = Clan.PlayerClan.Kingdom;

            Faction1Visual = new ImageIdentifierVM(BannerCode.CreateFrom(playerKingdom.Banner), true);
            Faction2Visual = new ImageIdentifierVM(BannerCode.CreateFrom(_opposingKingdom.Banner), true);

            PlayerWarExhaustion = $"{Instance!.GetWarExhaustion(Clan.PlayerClan.Kingdom, _opposingKingdom):F1}%";
            OpponentWarExhaustion = $"{Instance!.GetWarExhaustion(_opposingKingdom, Clan.PlayerClan.Kingdom):F1}%";

            PlayerWarExhaustionIsCritical = Instance!.HasCriticalWarExhaustion(Clan.PlayerClan.Kingdom, _opposingKingdom);
            OpponentWarExhaustionIsCritical = Instance!.HasCriticalWarExhaustion(_opposingKingdom, Clan.PlayerClan.Kingdom);

            PlayerCriticaExhaustionlHint = GetCriticaExhaustionlHint(PlayerWarExhaustionIsCritical, Clan.PlayerClan.Kingdom, _opposingKingdom);
            OpponentCriticaExhaustionlHint = GetCriticaExhaustionlHint(OpponentWarExhaustionIsCritical, _opposingKingdom, Clan.PlayerClan.Kingdom);

            var currentQuestState = Instance!.GetWarExhaustionQuestState(Clan.PlayerClan.Kingdom, _opposingKingdom);
            int questStateWarningHeight;
            switch (currentQuestState)
            {
                case ActiveQuestState.HasActiveQuest:
                    ActiveQuestWarning = _activeQuestWarning.ToString();
                    ActiveQuestHint = new(_activeQuestHint.SetTextVariable("CRITICAL_THRESHOLD", (int) (CriticalThresholdWarExhaustion * 100f)));
                    questStateWarningHeight = 16;
                    break;
                case ActiveQuestState.HadActiveQuest:
                    ActiveQuestWarning = _hadActiveQuestWarning.ToString();
                    ActiveQuestHint = new(_hadActiveQuestHint);
                    questStateWarningHeight = 16;
                    break;
                default:
                    ActiveQuestWarning = "";
                    ActiveQuestHint = new(TextObject.Empty);
                    questStateWarningHeight = 0;
                    break;
            }

            Stats = new DetailWarStatsVM(playerKingdom);
            OpponentStats = new DetailWarStatsVM(_opposingKingdom);

            var breakdowns = Instance.GetWarExhaustionBreakdown(playerKingdom, _opposingKingdom);

            Breakdowns.Clear();
            foreach (var breakdown in breakdowns) Breakdowns.Add(new WarExhaustionBreakdownVM(breakdown));

            ReportHeight = 480 + questStateWarningHeight + (breakdowns.Count - 4) * 16;
        }

        private static HintViewModel GetCriticaExhaustionlHint(bool isCrtitical, Kingdom faction1, Kingdom faction2)
        {
            return
                isCrtitical ? new(new(_criticaExhaustionlHintText, new()
                {
                    ["EXHAUSTED_FACTION"] = faction1.Name,
                    ["OTHER_FACTION"] = faction2.Name,
                    ["IS_CIVIL_WAR"] = (faction1.IsRebelKingdom() || faction2.IsRebelKingdom()) ? 1 : 0
                })) : new(TextObject.Empty);
        }

        [UsedImplicitly]
        private void OnComplete()
        {
            _finalize();
        }
    }
}