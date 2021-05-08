using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Diplomacy.CivilWar
{
    public abstract class RebelFaction
    {
        [SaveableProperty(1)]
        public Clan SponsorClan { get; private set; }

        [SaveableField(2)]
        private List<Clan> _participatingClans;

        [SaveableProperty(3)]
        public Kingdom ParentKingdom { get; private set; }

        [SaveableProperty(4)]
        public Kingdom? RebelKingdom { get; private set; }

        [SaveableProperty(5)]
        public bool AtWar { get; set; } = false;

        [SaveableProperty(6)]
        public CampaignTime DateStarted { get; private set; }

        [SaveableProperty(7)]
        public TextObject Name { get; private set; }

        [SaveableProperty(8)]
        public Dictionary<Town, Clan> OriginalFiefOwners { get; private set; }


        public RebelFaction(Clan sponsorClan)
        {
            _participatingClans = new();
            SponsorClan = sponsorClan;
            _participatingClans.Add(SponsorClan);
            ParentKingdom = sponsorClan.Kingdom;
            DateStarted = CampaignTime.Now;
            Name = FactionNameGenerator.GenerateFactionName(this.SponsorClan);
            OriginalFiefOwners = new();
        }
        public abstract RebelDemandType RebelDemandType { get; }

        public float FactionStrength { get { return _participatingClans.Select(c => c.TotalStrength).Sum(); } }
        public float LoyalistStrength { get { return ParentKingdom.TotalStrength - this.FactionStrength; } }

        public float RequiredStrengthRatio
        {
            get
            {
                var valor = SponsorClan.Leader.GetTraitLevel(DefaultTraits.Valor) + Math.Abs(DefaultTraits.Valor.MinValue);
                var maxRequiredStrengthRatio = 0.65f;
                var minRequiredStrengthRatio = 0.5f;
                var ratio = maxRequiredStrengthRatio - (((maxRequiredStrengthRatio - minRequiredStrengthRatio) / 4) * valor);
                return ratio;
            }
        }

        public float StrengthRatio => FactionStrength / ParentKingdom.TotalStrength;

        public bool HasCriticalSupport => StrengthRatio >= RequiredStrengthRatio;

        public void StartRebellion(Kingdom rebelKingdom)
        {
            this.AtWar = true;
            this.RebelKingdom = rebelKingdom;
            foreach (Town fief in ParentKingdom.Fiefs.Union(rebelKingdom.Fiefs))
            {
                OriginalFiefOwners[fief] = fief.OwnerClan;
            }
        }

        public void AddClan(Clan clan)
        {
            if (!_participatingClans.Contains(clan))
                _participatingClans.Add(clan);
        }

        public void RemoveClan(Clan clan)
        {
            if (_participatingClans.Contains(clan))
            {
                if (_participatingClans.Count == 1)
                {
                    RebelFactionManager.DestroyRebelFaction(this);
                    return;
                }

                if (clan == SponsorClan)
                {
                    SponsorClan = _participatingClans.Where(x => x != clan).GetRandomElementInefficiently();
                    Name = FactionNameGenerator.GenerateFactionName(this.SponsorClan);
                }
                _participatingClans.Remove(clan);
            }
        }

        public MBReadOnlyList<Clan> Clans { get => new MBReadOnlyList<Clan>(_participatingClans); }

        protected abstract void ApplyDemand();

        public virtual void EnforceFailure()
        {
            ApplyInfluenceChanges(false);

            ConsolidateKingdomsAction.Apply(this);
            RebelFactionManager.DestroyRebelFaction(this);


            InformationManager.ShowInquiry(
                new InquiryData(
                    new TextObject("{=0WVHMfN8}A Rebellion Crumbles").ToString(),
                    new TextObject("{=BVNGIAMM}{PARENT_KINGDOM} has crushed the rebellion ravaging their kingdom. {PLAYER_PARTICIPATION}")
                        .SetTextVariable("PARENT_KINGDOM", ParentKingdom.Name)
                        .SetTextVariable("PLAYER_PARTICIPATION", GetPlayerParticipationText(false))
                        .ToString(),
                    true,
                    false,
                    GameTexts.FindText("str_ok", null).ToString(),
                    null,
                    null,
                    null), true);
        }

        private void ApplyInfluenceChanges(bool success)
        {
            foreach (Clan clan in ParentKingdom.Clans)
            {
                if (clan == ParentKingdom.RulingClan)
                    clan.Influence = MBMath.ClampFloat(clan.Influence + (success ? LeaderInfluenceOnFailure : LeaderInfluenceOnSuccess), 0f, float.MaxValue);
                else
                    clan.Influence = MBMath.ClampFloat(clan.Influence + (success ? MemberInfluenceOnFailure : MemberInfluenceOnSuccess), 0f, float.MaxValue);
            }

            foreach (Clan clan in Clans)
            {
                if (clan == SponsorClan)
                    clan.Influence = MBMath.ClampFloat(clan.Influence + (success ? LeaderInfluenceOnSuccess : LeaderInfluenceOnFailure), 0f, float.MaxValue);
                else
                    clan.Influence = MBMath.ClampFloat(clan.Influence + (success ? MemberInfluenceOnSuccess : MemberInfluenceOnFailure), 0f, float.MaxValue);
            }
        }

        public abstract float LeaderInfluenceOnSuccess { get; }
        public abstract float MemberInfluenceOnSuccess { get; }
        public abstract float LeaderInfluenceOnFailure { get; }
        public abstract float MemberInfluenceOnFailure { get; }

        public void EnforceSuccess()
        {
            ApplyInfluenceChanges(true);
            ApplyDemand();
        }

        protected TextObject GetPlayerParticipationText(bool success)
        {
            TextObject text;
            if (SponsorClan == Clan.PlayerClan)
            {
                text = success
                    ? new TextObject("{=BPcKHP0D}As leader of the rebellion, you gained {INFLUENCE} influence.").SetTextVariable("INFLUENCE", Math.Abs(LeaderInfluenceOnSuccess))
                    : new TextObject("{=UoMvwnTb}As leader of the rebellion, you lost {INFLUENCE} influence.").SetTextVariable("INFLUENCE", Math.Abs(LeaderInfluenceOnFailure));
            }
            else if (Clans.Contains(Clan.PlayerClan))
            {
                text = success
                    ? new TextObject("{=QiX6m5cp}As a member of the rebellion, you gained {INFLUENCE} influence.").SetTextVariable("INFLUENCE", Math.Abs(MemberInfluenceOnSuccess))
                    : new TextObject("{=yrz8GMUb}As a member of the rebellion, you lost {INFLUENCE} influence.").SetTextVariable("INFLUENCE", Math.Abs(MemberInfluenceOnFailure));
            }
            else if(ParentKingdom.RulingClan == Clan.PlayerClan)
            {
                text = success
                    ? new TextObject("{=SNLviWvc}As leader of the loyalists, you lost {INFLUENCE} influence.").SetTextVariable("INFLUENCE", Math.Abs(LeaderInfluenceOnFailure))
                    : new TextObject("{=2I1X5fAC}As leader of the loyalists, you gained {INFLUENCE} influence.").SetTextVariable("INFLUENCE", Math.Abs(LeaderInfluenceOnSuccess));
            }        
            else if (ParentKingdom.Clans.Contains(Clan.PlayerClan))
            {
                text = success
                    ? new TextObject("{=4FfA8RL4}As a member of the loyalists, you lost {INFLUENCE} influence.").SetTextVariable("INFLUENCE", Math.Abs(MemberInfluenceOnFailure))
                    : new TextObject("{=uUu7DEDU}As a member of the loyalists, you gained {INFLUENCE} influence.").SetTextVariable("INFLUENCE", Math.Abs(MemberInfluenceOnSuccess));
            }
            else
            {
                text = TextObject.Empty;
            }
            return text;
        }

        public TextObject DemandDescription
        {
            get
            {
                TextObject desc;
                switch (this.RebelDemandType)
                {
                    case RebelDemandType.Secession:
                        desc = new TextObject("{=brEXAKDb}The rebels demand that their lands be allowed to secede from the kingdom.");
                        break;
                    case RebelDemandType.Abdication:
                        desc = new TextObject("{=8A6JPMWp}The rebels demand that {LEADER} abdicates their throne.").SetTextVariable("LEADER", this.ParentKingdom.Leader.Name);
                        break;
                    default:
                        desc = new TextObject("");
                        break;
                }
                return desc;
            }
        }

        public TextObject StatusText
        {
            get
            {
                return AtWar
                    ? new TextObject("{=ChzQncc0}Rebellion")
                    : new TextObject("{=WUAv0u4U}Gathering Support");
            }
        }

        public TextObject DemandText
        {
            get
            {
                return new TextObject("{=fw0k1KFl}Demand: {DEMAND_NAME}", new() { { "DEMAND_NAME", RebelDemandType.GetName() } });
            }
        }
    }
}