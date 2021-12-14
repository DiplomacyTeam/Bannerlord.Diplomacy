using Diplomacy.CivilWar.Actions;
using Diplomacy.CivilWar.Factions;

using JetBrains.Annotations;

using System;

using TaleWorlds.CampaignSystem;
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
        private RebelFactionParticipantVM _sponsorClan = null!;

        public RebelFaction RebelFaction { get; }

        [DataSourceProperty] public string JoinLabel { get; set; }

        [DataSourceProperty] public string LeaveLabel { get; set; }

        [DataSourceProperty] public string BOPLabel { get; }

        [DataSourceProperty] public string StartRebellionLabel { get; }
        [DataSourceProperty] public string Status { get; }

        [DataSourceProperty]
        public bool ShouldAllowJoin
        {
            get => _shouldAllowJoin;
            set
            {
                if (value != _shouldAllowJoin)
                {
                    _shouldAllowJoin = value;
                    OnPropertyChangedWithValue(value, nameof(ShouldAllowJoin));
                }
            }
        }

        [DataSourceProperty]
        public bool ShouldShowLeave
        {
            get => _shouldShowLeave;
            set
            {
                if (value != _shouldShowLeave)
                {
                    _shouldShowLeave = value;
                    OnPropertyChangedWithValue(value, nameof(ShouldShowLeave));
                }
            }
        }

        [DataSourceProperty]
        public bool ShouldShowStartRebellion
        {
            get => _shouldShowStartRebellion;
            set
            {
                if (value != _shouldShowStartRebellion)
                {
                    _shouldShowStartRebellion = value;
                    OnPropertyChangedWithValue(value, nameof(ShouldShowStartRebellion));
                }
            }
        }

        [DataSourceProperty]
        public bool ShouldAllowStartRebellion
        {
            get => _shouldAllowStartRebellion;
            set
            {
                if (value != _shouldAllowStartRebellion)
                {
                    _shouldAllowStartRebellion = value;
                    OnPropertyChangedWithValue(value, nameof(ShouldAllowStartRebellion));
                }
            }
        }

        [DataSourceProperty] public string ParticipantsText { get; set; }

        [DataSourceProperty] public string LeaderText { get; set; }
        [DataSourceProperty] public string FactionText { get; }
        [DataSourceProperty] public string KingdomText { get; }

        [DataSourceProperty]
        public string FactionName
        {
            get => _factionName;
            set
            {
                if (value != _factionName)
                {
                    _factionName = value;
                    OnPropertyChangedWithValue(value, nameof(FactionName));
                }
            }
        }

        [DataSourceProperty] public int BreakingPointOffset { get; set; }

        [DataSourceProperty]
        public int FactionStrength
        {
            get => _factionStrength;
            set
            {
                if (value != _factionStrength)
                {
                    _factionStrength = value;
                    OnPropertyChangedWithValue(value, nameof(FactionStrength));
                }
            }
        }

        [DataSourceProperty]
        public int LoyalistStrength
        {
            get => _loyalistStrength;
            set
            {
                if (value != _loyalistStrength)
                {
                    _loyalistStrength = value;
                    OnPropertyChangedWithValue(value, nameof(LoyalistStrength));
                }
            }
        }

        [DataSourceProperty] public int TotalStrength { get; private set; }

        [DataSourceProperty] public bool ShouldShowBalanceOfPower { get; private set; }

        [DataSourceProperty] public string Demand { get; set; }

        [DataSourceProperty] public string StartDate { get; set; }

        [DataSourceProperty]
        public MBBindingList<RebelFactionParticipantVM> ParticipatingClans
        {
            get => _participatingClans;
            set
            {
                if (value != _participatingClans)
                {
                    _participatingClans = value;
                    OnPropertyChangedWithValue(value, nameof(ParticipatingClans));
                }
            }
        }

        [DataSourceProperty]
        public RebelFactionParticipantVM SponsorClan
        {
            get => _sponsorClan;
            set
            {
                if (value != _sponsorClan)
                {
                    _sponsorClan = value;
                    OnPropertyChangedWithValue(value, nameof(SponsorClan));
                }
            }
        }

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
            foreach (Clan clan in RebelFaction.Clans)
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
        public void OnStartRebellion()
        {
            if (Game.Current.GameStateManager.ActiveState is KingdomState) Game.Current.GameStateManager.PopState();
            StartRebellionAction.Apply(RebelFaction);
            _refreshParent();
        }
    }
}