using Diplomacy.CivilWar.Actions;
using Diplomacy.CivilWar.Factions;
using Diplomacy.CivilWar.Scoring;
using Diplomacy.Extensions;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    public sealed class RebelFactionItemVM : TaleWorlds.Library.ViewModel
    {
        private static readonly TextObject _TJoinLabel = new("{=DZ6dpEn3}Join Faction");
        private static readonly TextObject _TLeaveLabel = new("{=uTCw0WiH}Leave Faction");
        private static readonly TextObject _TBOPLabel = new("{=1OUDq7Ul}Balance Of Power");
        private static readonly TextObject _TStartRebellionLabel = new("{=5q7TvMGL}Start Rebellion");
        private static readonly TextObject _TParticipantsText = new("{=VFwDRHc7}Participants");
        private readonly Action _onComplete;
        private readonly Action _refreshParent;
        private string _factionName = null!;
        private int _factionStrength;
        private int _loyalistStrength;
        private MBBindingList<RebelFactionParticipantVM> _participatingClans;
        private bool _shouldAllowJoin;
        private bool _shouldAllowStartRebellion;
        private bool _shouldShowLeave;
        private bool _shouldShowStartRebellion;
        private bool _shouldShowRecruitSupport;
        private RebelFactionParticipantVM _sponsorClan = null!;

        public RebelFaction RebelFaction { get; }

        [DataSourceProperty]
        public string JoinLabel { get; set; }

        [DataSourceProperty]
        public string LeaveLabel { get; set; }

        [DataSourceProperty]
        public string BOPLabel { get; }

        [DataSourceProperty]
        public string StartRebellionLabel { get; }
        [DataSourceProperty]
        public string Status { get; }

        [DataSourceProperty]
        public bool ShouldAllowJoin { get => _shouldAllowJoin; set => SetField(ref _shouldAllowJoin, value, nameof(ShouldAllowJoin)); }

        [DataSourceProperty]
        public bool ShouldShowLeave { get => _shouldShowLeave; set => SetField(ref _shouldShowLeave, value, nameof(ShouldShowLeave)); }

        [DataSourceProperty]
        public bool ShouldShowStartRebellion { get => _shouldShowStartRebellion; set => SetField(ref _shouldShowStartRebellion, value, nameof(ShouldShowStartRebellion)); }

        [DataSourceProperty]
        public bool ShouldAllowStartRebellion { get => _shouldAllowStartRebellion; set => SetField(ref _shouldAllowStartRebellion, value, nameof(ShouldAllowStartRebellion)); }

        [DataSourceProperty]
        public bool ShouldShowRecruitSupport { get => _shouldShowRecruitSupport; set => SetField(ref _shouldShowRecruitSupport, value, nameof(ShouldShowRecruitSupport)); }

        [DataSourceProperty]
        public string RecruitSupportLabel => new TextObject("{=FRrecBtn}Recruit Support").ToString();

        [DataSourceProperty]
        public string ParticipantsText { get; set; }

        [DataSourceProperty]
        public string LeaderText { get; set; }
        [DataSourceProperty]
        public string FactionText { get; }
        [DataSourceProperty]
        public string KingdomText { get; }

        [DataSourceProperty]
        public string FactionName { get => _factionName; set => SetField(ref _factionName, value, nameof(FactionName)); }

        [DataSourceProperty]
        public int BreakingPointOffset { get; set; }

        [DataSourceProperty]
        public int FactionStrength { get => _factionStrength; set => SetField(ref _factionStrength, value, nameof(FactionStrength)); }

        [DataSourceProperty]
        public int LoyalistStrength { get => _loyalistStrength; set => SetField(ref _loyalistStrength, value, nameof(LoyalistStrength)); }

        [DataSourceProperty]
        public int TotalStrength { get; private set; }

        [DataSourceProperty]
        public bool ShouldShowBalanceOfPower { get; private set; }

        [DataSourceProperty]
        public string Demand { get; set; }

        [DataSourceProperty]
        public string StartDate { get; set; }

        [DataSourceProperty]
        public MBBindingList<RebelFactionParticipantVM> ParticipatingClans { get => _participatingClans; set => SetField(ref _participatingClans, value, nameof(ParticipatingClans)); }

        [DataSourceProperty]
        public RebelFactionParticipantVM SponsorClan { get => _sponsorClan; set => SetField(ref _sponsorClan, value, nameof(SponsorClan)); }

        public RebelFactionItemVM(RebelFaction rebelFaction, Action onComplete, Action refreshParent)
        {
            RebelFaction = rebelFaction;
            _onComplete = onComplete;

            _participatingClans = new MBBindingList<RebelFactionParticipantVM>();
            StartDate = new TextObject("{=Ysrttgis}Start Date: {CAMPAIGN_TIME}").SetTextVariable("CAMPAIGN_TIME", RebelFaction.DateStarted.ToString())
                .ToString();
            Demand = new TextObject("{=o3w3jNzZ}Demand: {DEMAND_NAME}").SetTextVariable("DEMAND_NAME", RebelFaction.RebelDemandType.GetName())
                .ToString();
            Status = new TextObject("{=hbPiVQVK}Status: {STATUS}").SetTextVariable("STATUS", RebelFaction.StatusText).ToString();
            JoinLabel = _TJoinLabel.ToString();
            LeaveLabel = _TLeaveLabel.ToString();
            BOPLabel = _TBOPLabel.ToString();
            StartRebellionLabel = _TStartRebellionLabel.ToString();
            ParticipantsText = _TParticipantsText.ToString();
            LeaderText = GameTexts.FindText("str_leader").ToString();
            FactionText = GameTexts.FindText("str_faction").ToString();
            KingdomText = GameTexts.FindText("str_kingdom").ToString();
            _refreshParent = refreshParent;
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            ParticipatingClans.Clear();
            foreach (var clan in RebelFaction.Clans)
            {
                var vm = new RebelFactionParticipantVM(clan, RebelFaction, _onComplete);
                if (clan == RebelFaction.SponsorClan)
                    SponsorClan = vm;
                else
                    ParticipatingClans.Add(vm);
            }

            ShouldAllowJoin = JoinFactionAction.CanApply(Clan.PlayerClan, RebelFaction, out _);

            ShouldShowLeave = RebelFaction.Clans.Contains(Hero.MainHero.Clan);
            FactionStrength = (int) Math.Round(RebelFaction.FactionStrength);
            LoyalistStrength = (int) Math.Round(RebelFaction.LoyalistStrength);
            TotalStrength = FactionStrength + LoyalistStrength;
            ShouldShowBalanceOfPower = !RebelFaction.AtWar;
            ShouldShowStartRebellion = RebelFaction.SponsorClan == Clan.PlayerClan;
            ShouldAllowStartRebellion = ShouldShowStartRebellion && FactionStrength > LoyalistStrength;
            ShouldShowRecruitSupport = Settings.Instance!.EnablePlayerFactionRecruitment
                                       && RebelFaction.SponsorClan == Clan.PlayerClan && Clan.PlayerClan.Leader == Hero.MainHero
                                       && !RebelFaction.ParentKingdom.GetRebelFactions().Any(f => f.AtWar);
            FactionName = RebelFaction.Name.ToString();

            var bopSize = 300;
            var offset = Convert.ToInt32((RebelFaction.RequiredStrengthRatio - 0.5f) * bopSize);
            BreakingPointOffset = offset;
        }

        [UsedImplicitly]
        public void OnJoin()
        {
            RebelFaction.AddClan(Clan.PlayerClan);
            RefreshValues();
        }

        [UsedImplicitly]
        public void OnLeave()
        {
            RebelFaction.RemoveClan(Clan.PlayerClan);
            _refreshParent();
        }

        [UsedImplicitly]
        public void OnRecruitSupport()
        {
            RefreshValues();
            if (!ShouldShowRecruitSupport
                || RebelFaction.ParentKingdom != Clan.PlayerClan.Kingdom)
                return;
            var candidates = new List<InquiryElement>();
            var scores = RebelFaction.ParentKingdom.Clans.Where(c => c != Clan.PlayerClan && !c.IsEliminated
                         && !c.IsMinorFaction && !c.IsUnderMercenaryService && c.Leader is not null && !RebelFaction.Clans.Contains(c))
                .Select(c => (Clan: c, Score: RebelFactionScoringModel.GetDemandScore(c, RebelFaction)))
                .OrderByDescending(c => c.Score.ResultNumber);
            foreach (var candidate in scores)
            {
                var clan = candidate.Clan;
                var score = candidate.Score;
                var hint = new TextObject("{=FRcanHint}Military strength: {STRENGTH}{newline}Support: {SCORE} / {REQUIRED}{newline}{REASONS}")
                    .SetTextVariable("STRENGTH", (int) clan.CurrentTotalStrength)
                    .SetTextVariable("SCORE", (int) score.ResultNumber)
                    .SetTextVariable("REQUIRED", (int) RebelFactionScoringModel.RequiredScore)
                    .SetTextVariable("REASONS", string.Join(Environment.NewLine, score.GetLines().Select(l => $"{l.Item1}: {l.Item2:+0.##;-0.##;0}")));
                if (!RecruitFactionSupportAction.CanRecruit(clan, RebelFaction, out var reason)
                    || (score.ResultNumber < RebelFactionScoringModel.RequiredScore
                        && !RecruitFactionSupportAction.CanPersuade(clan, RebelFaction, out reason)))
                    hint = new TextObject("{=FRcanBlock}{DETAILS}{newline}{REASON}").SetTextVariable("DETAILS", hint).SetTextVariable("REASON", reason);
                var label = new TextObject("{=FRcanName}{CLAN} - Support: {SCORE} / {REQUIRED}")
                    .SetTextVariable("CLAN", clan.Name).SetTextVariable("SCORE", (int) score.ResultNumber)
                    .SetTextVariable("REQUIRED", (int) RebelFactionScoringModel.RequiredScore);
                candidates.Add(new InquiryElement(clan.Leader, label.ToString(), null, true, hint.ToString()));
            }
            if (candidates.Count == 0)
            {
                InformationManager.ShowInquiry(new InquiryData(RecruitSupportLabel,
                    new TextObject("{=FRnoClans}There are no other clans to approach in this kingdom.").ToString(), true, false,
                    GameTexts.FindText("str_ok").ToString(), null, null, null), true);
                return;
            }
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                RecruitSupportLabel,
                new TextObject("{=FRcanDesc}Choose a clan leader to approach. Speak to them about supporting your faction, in person or through Send Messenger on their encyclopedia page.").ToString(),
                candidates, true, 1, 1, new TextObject("{=FRviewLdr}View Clan Leader").ToString(),
                GameTexts.FindText("str_cancel").ToString(), selected =>
                {
                    if (selected.FirstOrDefault()?.Identifier is Hero hero)
                    {
                        Campaign.Current.EncyclopediaManager.GoToLink(hero.EncyclopediaLink);
                        _onComplete();
                    }
                }, null), true);
        }

        [UsedImplicitly]
        public void OnStartRebellion()
        {
            if (Game.Current.GameStateManager.ActiveState is KingdomState) Game.Current.GameStateManager.PopState();
            StartRebellionAction.Apply(RebelFaction);
            _refreshParent();
        }
    }
}