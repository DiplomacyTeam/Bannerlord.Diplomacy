using Diplomacy.CivilWar.Factions;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar
{
    internal sealed class AbdicationDemandScore : AbstractFactionDemandScoringModel
    {
        public float FiefDeficit => 10;

        private static readonly TextObject _TFiefDeficit = new TextObject(StringConstants.NotEnoughFiefs);
        private static readonly TextObject _TRulerDishonorable = new TextObject("{=hdHW8IxR}Ruler is Dishonorable");
        private static readonly TextObject _TRulerCowardly = new TextObject("{=CxbS1oLN}Ruler is Cowardly");

        public AbdicationDemandScore() : base(new AbdicationScores()) { }

        protected override List<RebelFaction> GetPossibleFactions(Clan clan)
        {
            return new List<RebelFaction>() { new AbdicationFaction(clan) };
        }

        protected override IEnumerable<Tuple<TextObject, float>> GetMemberScore(Clan clan, RebelFaction rebelFaction)
        {
            List<Tuple<TextObject, float>> memberScores = new();
            var generosityMultiplier = Math.Abs(clan.Leader.GetTraitLevel(DefaultTraits.Generosity) - DefaultTraits.Generosity.MaxValue);
            var scorePerFiefDeficit = generosityMultiplier * FiefDeficit;
            // happy with fiefs
            float fiefDeficit = (clan.Tier - 1 - clan.Fiefs.Count);
            if (fiefDeficit > 0)
            {
                memberScores.Add(new Tuple<TextObject, float>(_TFiefDeficit, fiefDeficit * scorePerFiefDeficit));
            }

            memberScores.AddRange(AddTraitScore(clan, rebelFaction, DefaultTraits.Honor, _TRulerDishonorable));
            memberScores.AddRange(AddTraitScore(clan, rebelFaction, DefaultTraits.Valor, _TRulerCowardly));

            return memberScores;
        }

        private IEnumerable<Tuple<TextObject, float>> AddTraitScore(Clan clan, RebelFaction rebelFaction, TraitObject trait, TextObject rulerText)
        {
            Hero clanLeader = clan.Leader;
            Hero ruler = rebelFaction.ParentKingdom.Leader;
            if (clanLeader.GetTraitLevel(trait) > 0)
            {
                int rulerTrait = ruler.GetTraitLevel(trait);
                if (rulerTrait < 0)
                {
                    yield return new Tuple<TextObject, float>(rulerText, Math.Abs(rulerTrait) * 10f);
                }
            }
        }

        protected override IEnumerable<Tuple<TextObject, float>> GetLeaderOnlyScore(Clan clan, RebelFaction rebelFaction)
        {
            // only one abdication faction
            if (RebelFactionManager.GetRebelFaction(clan.Kingdom).Any(x => x.RebelDemandType == RebelDemandType.Abdication && x.SponsorClan != clan))
                yield return new Tuple<TextObject, float>(new TextObject(), -999);
        }

        protected override IEnumerable<Tuple<TextObject, float>> GetMemberOnlyScore(Clan clan, RebelFaction rebelFaction)
        {
            yield break;
        }

        public class AbdicationScores : IFactionDemandScores
        {
            public float Base => 20;
            public float NegativeLeaderRelationshipWeight => 0;

            public float KingdomSizeScoreMax => 50;
            public float KingdomSizeCastleScore => 1;
            public float KingdomSizeTownScore => 2;
        }
    }
}