using Diplomacy.CivilWar.Actions;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace Diplomacy.CivilWar.Factions
{
    public abstract class RebelFaction
    {
        [SaveableProperty(1)]
        public Clan SponsorClan { get; private set; }

        [SaveableField(2)]
        [UsedImplicitly]
        private List<Clan> _participatingClans;

        [SaveableProperty(3)]
        [UsedImplicitly]
        public Kingdom ParentKingdom { get; private set; }

        [SaveableProperty(4)]
        public Kingdom? RebelKingdom { get; private set; }

        [SaveableProperty(5)]
        public bool AtWar { get; set; }

        [SaveableProperty(6)]
        [UsedImplicitly]
        public CampaignTime DateStarted { get; private set; }

        [SaveableProperty(7)]
        public TextObject Name { get; private set; }

        [SaveableProperty(8)]
        [UsedImplicitly]
        public Dictionary<Town, Clan> OriginalFiefOwners { get; private set; }

        public abstract RebelDemandType RebelDemandType { get; }

        public abstract bool ConsolidateOnSuccess { get; }

        public float FactionStrength
        {
            get { return _participatingClans.Select(c => c.TotalStrength).Sum(); }
        }

        public float LoyalistStrength => ParentKingdom.TotalStrength - FactionStrength;

        public float RequiredStrengthRatio
        {
            get
            {
                var valor = SponsorClan.Leader.GetTraitLevel(DefaultTraits.Valor) + Math.Abs(DefaultTraits.Valor.MinValue);
                var maxRequiredStrengthRatio = 0.65f;
                var minRequiredStrengthRatio = 0.5f;
                var ratio = maxRequiredStrengthRatio - (maxRequiredStrengthRatio - minRequiredStrengthRatio) / 4 * valor;
                return ratio;
            }
        }

        public float StrengthRatio => FactionStrength / ParentKingdom.TotalStrength;

        public bool HasCriticalSupport => StrengthRatio >= RequiredStrengthRatio;

        public MBReadOnlyList<Clan> Clans => new(_participatingClans);

        public abstract float LeaderInfluenceOnSuccess { get; }
        public abstract float MemberInfluenceOnSuccess { get; }
        public abstract float LeaderInfluenceOnFailure { get; }
        public abstract float MemberInfluenceOnFailure { get; }

        public TextObject DemandDescription
        {
            get
            {
                TextObject desc;
                switch (RebelDemandType)
                {
                    case RebelDemandType.Secession:
                        desc = new TextObject("{=brEXAKDb}The rebels demand that their lands be allowed to secede from the kingdom.");
                        break;
                    case RebelDemandType.Abdication:
                        desc = new TextObject("{=8A6JPMWp}The rebels demand that {LEADER} abdicates their throne.").SetTextVariable("LEADER",
                            ParentKingdom.Leader.Name);
                        break;
                    default:
                        desc = new TextObject();
                        break;
                }

                return desc;
            }
        }

        public TextObject StatusText =>
            AtWar
                ? new TextObject("{=ChzQncc0}Rebellion")
                : new TextObject("{=WUAv0u4U}Gathering Support");

        protected RebelFaction(Clan sponsorClan)
        {
            _participatingClans = new List<Clan>();
            SponsorClan = sponsorClan;
            _participatingClans.Add(SponsorClan);
            ParentKingdom = sponsorClan.Kingdom;
            DateStarted = CampaignTime.Now;
            Name = FactionNameGenerator.GenerateFactionName(SponsorClan);
            OriginalFiefOwners = new Dictionary<Town, Clan>();
        }

        public void StartRebellion(Kingdom rebelKingdom)
        {
            AtWar = true;
            RebelKingdom = rebelKingdom;
            foreach (var fief in ParentKingdom.Fiefs.Union(rebelKingdom.Fiefs)) OriginalFiefOwners[fief] = fief.OwnerClan;
        }

        public void AddClan(Clan clan)
        {
            if (clan != null && !_participatingClans.Contains(clan))
                _participatingClans.Add(clan);
        }

        public void RemoveClan(Clan clan)
        {
            if (_participatingClans.Contains(clan))
            {
                var remainingClanList = _participatingClans.Where(x => x != clan && !x.IsEliminated).ToList();
                if (!remainingClanList.Any())
                {
                    RebelFactionManager.DestroyRebelFaction(this);
                    return;
                }

                if (clan == SponsorClan)
                {
                    SponsorClan = remainingClanList.GetRandomElementInefficiently();
                    Name = FactionNameGenerator.GenerateFactionName(SponsorClan);
                }

                _participatingClans.Remove(clan);
            }
        }

        protected abstract void ApplyDemand();

        public virtual void EnforceFailure()
        {
            ApplyInfluenceAndReputationChanges(false);

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
                    GameTexts.FindText("str_ok").ToString(),
                    null,
                    null,
                    null), true);
        }

        private void ApplyInfluenceAndReputationChanges(bool success)
        {
            ApplyInfluenceChanges(success);
            ApplyReputationChanges();
        }

        private void ApplyReputationChanges()
        {
            var loyalistCombinations = from clan in ParentKingdom.Clans
                                       from otherClan in ParentKingdom.Clans
                                       where clan.Id < otherClan.Id
                                       select Tuple.Create(clan, otherClan);

            var rebelCombinations = from clan in Clans
                                    from otherClan in Clans
                                    where clan.Id < otherClan.Id
                                    select Tuple.Create(clan, otherClan);

            var opposingCombinations = from clan in ParentKingdom.Clans
                                       from otherClan in Clans
                                       select Tuple.Create(clan, otherClan);

            foreach (Tuple<Clan, Clan> tuple in loyalistCombinations)
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(tuple.Item1.Leader, tuple.Item2.Leader,
                    tuple.Item1 == SponsorClan || tuple.Item2 == SponsorClan ? 10 : 5);

            foreach (var tuple in rebelCombinations)
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(tuple.Item1.Leader, tuple.Item2.Leader,
                    tuple.Item1 == SponsorClan || tuple.Item2 == SponsorClan ? 10 : 5);

            foreach (Tuple<Clan, Clan> tuple in opposingCombinations)
            {
                var hasSponsorClan = tuple.Item1 == SponsorClan || tuple.Item2 == SponsorClan;
                var hasRulerClan = tuple.Item1 == ParentKingdom.RulingClan || tuple.Item2 == ParentKingdom.RulingClan;
                int value;
                if (hasSponsorClan && hasRulerClan)
                    value = -20;
                else if (hasSponsorClan || hasSponsorClan) // TODO: maybe hasSponsorClan || hasRulerClan ?
                    value = -10;
                else
                    value = -5;

                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(tuple.Item1.Leader, tuple.Item2.Leader, value);
            }
        }

        private void ApplyInfluenceChanges(bool success)
        {
            foreach (var clan in ParentKingdom.Clans)
                clan.Influence = clan == ParentKingdom.RulingClan
                    ? MBMath.ClampFloat(clan.Influence + (success ? LeaderInfluenceOnFailure : LeaderInfluenceOnSuccess), 0f, float.MaxValue)
                    : MBMath.ClampFloat(clan.Influence + (success ? MemberInfluenceOnFailure : MemberInfluenceOnSuccess), 0f, float.MaxValue);

            foreach (var clan in Clans)
                clan.Influence = clan == SponsorClan
                    ? MBMath.ClampFloat(clan.Influence + (success ? LeaderInfluenceOnSuccess : LeaderInfluenceOnFailure), 0f, float.MaxValue)
                    : MBMath.ClampFloat(clan.Influence + (success ? MemberInfluenceOnSuccess : MemberInfluenceOnFailure), 0f, float.MaxValue);
        }

        public void EnforceSuccess()
        {
            ApplyInfluenceAndReputationChanges(true);
            ApplyDemand();
        }

        protected TextObject GetPlayerParticipationText(bool success)
        {
            TextObject text;
            if (SponsorClan == Clan.PlayerClan)
                text = success
                    ? new TextObject("{=BPcKHP0D}As leader of the rebellion, you gained {INFLUENCE} influence.").SetTextVariable("INFLUENCE",
                        Math.Abs(LeaderInfluenceOnSuccess))
                    : new TextObject("{=UoMvwnTb}As leader of the rebellion, you lost {INFLUENCE} influence.").SetTextVariable("INFLUENCE",
                        Math.Abs(LeaderInfluenceOnFailure));
            else if (Clans.Contains(Clan.PlayerClan))
                text = success
                    ? new TextObject("{=QiX6m5cp}As a member of the rebellion, you gained {INFLUENCE} influence.").SetTextVariable("INFLUENCE",
                        Math.Abs(MemberInfluenceOnSuccess))
                    : new TextObject("{=yrz8GMUb}As a member of the rebellion, you lost {INFLUENCE} influence.").SetTextVariable("INFLUENCE",
                        Math.Abs(MemberInfluenceOnFailure));
            else if (ParentKingdom.RulingClan == Clan.PlayerClan)
                text = success
                    ? new TextObject("{=SNLviWvc}As leader of the loyalists, you lost {INFLUENCE} influence.").SetTextVariable("INFLUENCE",
                        Math.Abs(LeaderInfluenceOnFailure))
                    : new TextObject("{=2I1X5fAC}As leader of the loyalists, you gained {INFLUENCE} influence.").SetTextVariable("INFLUENCE",
                        Math.Abs(LeaderInfluenceOnSuccess));
            else if (ParentKingdom.Clans.Contains(Clan.PlayerClan))
                text = success
                    ? new TextObject("{=4FfA8RL4}As a member of the loyalists, you lost {INFLUENCE} influence.").SetTextVariable("INFLUENCE",
                        Math.Abs(MemberInfluenceOnFailure))
                    : new TextObject("{=uUu7DEDU}As a member of the loyalists, you gained {INFLUENCE} influence.").SetTextVariable("INFLUENCE",
                        Math.Abs(MemberInfluenceOnSuccess));
            else
                text = TextObject.Empty;
            return text;
        }
    }
}