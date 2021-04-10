using Diplomacy.CivilWar;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    public class RebelFactionItemVM : TaleWorlds.Library.ViewModel
    {
        private MBBindingList<RebelFactionParticipantVM> _participatingClans;
        private RebelFactionParticipantVM _sponsorClan;
        private int _loyalistStrength;
        private int _factionStrength;
        private Action _onComplete;
        private Action _refreshParent;
        private bool _shouldAllowJoin;
        private bool _shouldShowLeave;
        private bool _shouldAllowStartRebellion;
        private bool _shouldShowStartRebellion;
        private string _factionName;

        private static TextObject _SJoinLabel = new TextObject("{=DZ6dpEn3}Join Faction");
        private static TextObject _SLeaveLabel = new TextObject("{=uTCw0WiH}Leave Faction");
        private static TextObject _SBOPLabel = new TextObject("{=1OUDq7Ul}Balance Of Power");
        private static TextObject _SStartRebellionLabel = new TextObject("{=5q7TvMGL}Start Rebellion");
        private static TextObject _SParticipantsText = new TextObject("{=VFwDRHc7}Participants");

        public RebelFaction RebelFaction { get; }

        public RebelFactionItemVM(RebelFaction rebelFaction, Action onComplete, Action refreshParent)
        {
            RebelFaction = rebelFaction;
            this._onComplete = onComplete;

            _participatingClans = new();
            StartDate = new TextObject("{=Ysrttgis}Start Date: {CAMPAIGN_TIME}").SetTextVariable("CAMPAIGN_TIME", RebelFaction.DateStarted.ToString()).ToString();
            Demand = new TextObject("{=o3w3jNzZ}Demand: {DEMAND_NAME}").SetTextVariable("DEMAND_NAME", RebelFaction.RebelDemandType.GetName()).ToString();
            Status = new TextObject("{=hbPiVQVK}Status: {STATUS}").SetTextVariable("STATUS", RebelFaction.StatusText).ToString();
            JoinLabel = _SJoinLabel.ToString();
            LeaveLabel = _SLeaveLabel.ToString();
            BOPLabel = _SBOPLabel.ToString();
            StartRebellionLabel = _SStartRebellionLabel.ToString();
            ParticipantsText = _SParticipantsText.ToString();
            LeaderText = GameTexts.FindText("str_leader").ToString();
            FactionText = GameTexts.FindText("str_faction").ToString();
            KingdomText = GameTexts.FindText("str_kingdom").ToString();
            this._refreshParent = refreshParent;
            this.RefreshValues();
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

            this.ShouldAllowJoin = RebelFaction.ParentKingdom == Clan.PlayerClan.Kingdom
                && !Hero.MainHero.IsFactionLeader
                && !RebelFaction.Clans.Contains(Clan.PlayerClan)
                && RebelFaction.SponsorClan != Clan.PlayerClan
                && !RebelFaction.AtWar;

            this.ShouldShowLeave = RebelFaction.Clans.Contains(Hero.MainHero.Clan);
            this.FactionStrength = (int)Math.Round(RebelFaction.FactionStrength);
            this.LoyalistStrength = (int)Math.Round(RebelFaction.LoyalistStrength);
            this.TotalStrength = FactionStrength + LoyalistStrength;
            this.ShouldShowBalanceOfPower = !RebelFaction.AtWar;
            this.ShouldShowStartRebellion = RebelFaction.SponsorClan == Clan.PlayerClan;
            this.ShouldAllowStartRebellion = ShouldShowStartRebellion && FactionStrength > LoyalistStrength;
            this.FactionName = RebelFaction.Name.ToString();

            var bopSize = 300;
            var offset = Convert.ToInt32((this.RebelFaction.RequiredStrengthRatio - 0.5f) * bopSize / 2);
            this.BreakingPointOffset = offset;
        }

        public void OnJoin()
        {
            this.RebelFaction.AddClan(Clan.PlayerClan);
            this.RefreshValues();
        }

        public void OnLeave()
        {
            this.RebelFaction.RemoveClan(Clan.PlayerClan);
            _refreshParent();
        }

        public void OnStartRebellion()
        {
            StartRebellionAction.Apply(RebelFaction);
            _refreshParent();
        }

        [DataSourceProperty]
        public string JoinLabel { get; set; }

        [DataSourceProperty]
        public string LeaveLabel { get; set; }

        [DataSourceProperty]
        public string BOPLabel { get; private set; }

        [DataSourceProperty]
        public string StartRebellionLabel { get; private set; }
        [DataSourceProperty]
        public string Status { get; private set; }

        [DataSourceProperty]
        public bool ShouldAllowJoin
        {
            get
            {
                return this._shouldAllowJoin;
            }
            set
            {
                if (value != this._shouldAllowJoin)
                {
                    this._shouldAllowJoin = value;
                    base.OnPropertyChangedWithValue(value, nameof(ShouldAllowJoin));
                }
            }
        }

        [DataSourceProperty]
        public bool ShouldShowLeave
        {
            get
            {
                return this._shouldShowLeave;
            }
            set
            {
                if (value != this._shouldShowLeave)
                {
                    this._shouldShowLeave = value;
                    base.OnPropertyChangedWithValue(value, nameof(ShouldShowLeave));
                }
            }
        }

        [DataSourceProperty]
        public bool ShouldShowStartRebellion
        {
            get
            {
                return this._shouldShowStartRebellion;
            }
            set
            {
                if (value != this._shouldShowStartRebellion)
                {
                    this._shouldShowStartRebellion = value;
                    base.OnPropertyChangedWithValue(value, nameof(ShouldShowStartRebellion));
                }
            }
        }

        [DataSourceProperty]
        public bool ShouldAllowStartRebellion
        {
            get
            {
                return this._shouldAllowStartRebellion;
            }
            set
            {
                if (value != this._shouldAllowStartRebellion)
                {
                    this._shouldAllowStartRebellion = value;
                    base.OnPropertyChangedWithValue(value, nameof(ShouldAllowStartRebellion));
                }
            }
        }

        [DataSourceProperty]
        public string ParticipantsText { get; set; }

        [DataSourceProperty]
        public string LeaderText { get; set; }
        
        [DataSourceProperty]
        public string FactionText { get; private set; }
        [DataSourceProperty]
        public string KingdomText { get; private set; }
        
        [DataSourceProperty]
        public string FactionName
        {
            get
            {
                return this._factionName;
            }
            set
            {
                if (value != this._factionName)
                {
                    this._factionName = value;
                    base.OnPropertyChangedWithValue(value, nameof(FactionName));
                }
            }

        }

        [DataSourceProperty]
        public int BreakingPointOffset { get; set; }

        [DataSourceProperty]
        public int FactionStrength
        {
            get
            {
                return this._factionStrength;
            }
            set
            {
                if (value != this._factionStrength)
                {
                    this._factionStrength = value;
                    base.OnPropertyChangedWithValue(value, nameof(FactionStrength));
                }
            }

        }
        [DataSourceProperty]
        public int LoyalistStrength
        {
            get
            {
                return this._loyalistStrength;
            }
            set
            {
                if (value != this._loyalistStrength)
                {
                    this._loyalistStrength = value;
                    base.OnPropertyChangedWithValue(value, nameof(LoyalistStrength));
                }
            }
        }
        [DataSourceProperty]
        public int TotalStrength { get; private set; }

        [DataSourceProperty]
        public bool ShouldShowBalanceOfPower { get; private set; }

        [DataSourceProperty]
        public string Demand { get; set; }

        [DataSourceProperty]
        public string StartDate { get; set; }

        [DataSourceProperty]
        public MBBindingList<RebelFactionParticipantVM> ParticipatingClans
        {
            get
            {
                return this._participatingClans;
            }
            set
            {
                if (value != this._participatingClans)
                {
                    this._participatingClans = value;
                    base.OnPropertyChangedWithValue(value, nameof(ParticipatingClans));
                }
            }
        }

        [DataSourceProperty]
        public RebelFactionParticipantVM SponsorClan
        {
            get
            {
                return this._sponsorClan;
            }
            set
            {
                if (value != this._sponsorClan)
                {
                    this._sponsorClan = value;
                    base.OnPropertyChangedWithValue(value, nameof(SponsorClan));
                }
            }
        }
    }
}