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
        public Kingdom? RebelKingdom { get; set; }

        [SaveableProperty(5)]
        public bool AtWar { get; set; } = false;

        [SaveableProperty(6)]
        public CampaignTime DateStarted { get; private set; }

        [SaveableProperty(7)]
        public TextObject Name { get; private set; }

        public RebelFaction(Clan sponsorClan)
        {
            _participatingClans = new();
            SponsorClan = sponsorClan;
            _participatingClans.Add(SponsorClan);
            ParentKingdom = sponsorClan.Kingdom;
            DateStarted = CampaignTime.Now;
            Name = FactionNameGenerator.GenerateFactionName(this.SponsorClan);
        }
        public abstract RebelDemandType RebelDemandType { get; }

        public float FactionStrength { get { return _participatingClans.Select(c => c.TotalStrength).Sum(); } }
        public float LoyalistStrength { get { return ParentKingdom.TotalStrength - this.FactionStrength; } }

        public float RequiredStrengthRatio { 
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

        public abstract void EnforceDemand();

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