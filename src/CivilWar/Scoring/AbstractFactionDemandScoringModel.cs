using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar
{
    public abstract class AbstractFactionDemandScoringModel
    {
        public IFactionDemandScores Scores { get; init; }
        public AbstractFactionDemandScoringModel(IFactionDemandScores scores) => Scores = scores;

        private static readonly TextObject _TFactionTendency = new TextObject("{=Vp9EcnuC}Faction Tendency");
        private static readonly TextObject _TRelationsFactionTarget = new TextObject("{=dr1OCcmL}Relations: Target");
        private static readonly TextObject _TRelationsFactionLeader = new TextObject("{=mouJZAAy}Relations: Leader");
        private static readonly TextObject _TKingdomSize = new TextObject("{=FtX4LPXF}Kingdom Size");
        private static readonly TextObject _TValorPenalty = new TextObject("{=F8E3GKgp}Trait: Valor");

        public ExplainedNumber GetScore(Clan clan, RebelFaction rebelFaction)
        {
            List<Tuple<TextObject, float>> scores = new();
            scores.AddRange(GetMemberScore(clan, rebelFaction));

            if (rebelFaction.SponsorClan == clan)
            {
                scores.AddRange(GetLeaderOnlyScore(clan, rebelFaction));
            }
            else
            {
                scores.AddRange(GetMemberOnlyScore(clan, rebelFaction));
            }
            scores.AddRange(GetBaseScore(clan, rebelFaction));

            var score = new ExplainedNumber(Scores.Base, true);

            foreach (Tuple<TextObject, float> scoreTuple in scores)
            {
                score.Add(scoreTuple.Item2, scoreTuple.Item1);
            }

            score.Add(Settings.Instance!.FactionTendency, _TFactionTendency);

            return score;
        }

        public Tuple<RebelFaction, ExplainedNumber> GetScore(Clan clan)
        {
            Dictionary<RebelFaction, ExplainedNumber> possibilityScores = new();
            foreach (RebelFaction faction in GetPossibleFactions(clan))
            {
                possibilityScores[faction] = GetScore(clan, faction);
            }

            var bestFaction = possibilityScores.OrderByDescending(x => x.Value.ResultNumber).First();

            return new Tuple<RebelFaction, ExplainedNumber>(bestFaction.Key, bestFaction.Value);
        }

        protected abstract List<RebelFaction> GetPossibleFactions(Clan clan);

        protected abstract IEnumerable<Tuple<TextObject, float>> GetMemberScore(Clan clan, RebelFaction rebelFaction);
        protected abstract IEnumerable<Tuple<TextObject, float>> GetMemberOnlyScore(Clan clan, RebelFaction rebelFaction);
        protected abstract IEnumerable<Tuple<TextObject, float>> GetLeaderOnlyScore(Clan clan, RebelFaction rebelFaction);

        protected virtual IEnumerable<Tuple<TextObject, float>> GetBaseScore(Clan clan, RebelFaction rebelFaction)
        {
            var kingdom = clan.Kingdom;

            // relation with ruling clan
            var relationships = GetRelationshipScores(clan, rebelFaction);
            foreach (Tuple<TextObject, float> relationship in relationships)
                yield return relationship;

            // valor penalty
            int[] valorPenalty = { -30, -20, -10, -10, -10 };
            float[] strengthThreshold = { 0.4f, 0.3f, 0.2f, 0.1f, 0.0f };
            var normalizedValor = clan.Leader.GetTraitLevel(DefaultTraits.Valor) + DefaultTraits.Valor.MaxValue;
            var valorScore = rebelFaction.StrengthRatio < strengthThreshold[normalizedValor] ? valorPenalty[normalizedValor] : 0f;
            yield return new Tuple<TextObject, float>(_TValorPenalty, valorScore);


            // kingdom size
            var towns = new List<Town>(Town.AllFiefs);
            float totalFiefsCount = towns.Count;
            float kingdomFiefsCount = kingdom.Fiefs.Count;

            var kingdomCastles = kingdom.Fiefs.Where(x => x.IsCastle).Count();
            var kingdomTowns = kingdom.Fiefs.Where(x => x.IsTown).Count();

            var kingdomSize = kingdomCastles + (kingdomTowns * 2);
            var kingdomSizeScore = MBMath.ClampFloat(kingdomSize, 0, Scores.KingdomSizeScoreMax);

            yield return new Tuple<TextObject, float>(_TKingdomSize, kingdomSizeScore);
        }

        public interface IFactionDemandScores
        {
            public float KingdomSizeScoreMax { get; }
            public float KingdomSizeCastleScore { get; }
            public float KingdomSizeTownScore { get; }
            public float Base { get; }
            public float NegativeLeaderRelationshipWeight { get; }
        }

        protected virtual IEnumerable<Tuple<TextObject, float>> GetRelationshipScores(Clan clan, RebelFaction rebelFaction)
        {
            List<Tuple<TextObject, float>> scores = new();
            var mercyMultiplier = 1 - (clan.Leader.GetTraitLevel(DefaultTraits.Mercy) * 0.25f);
            var relationshipWithFactionLeader = clan != rebelFaction.SponsorClan ? clan.GetRelationWithClan(rebelFaction.SponsorClan) : 0f;

            var relationshipWithFactionLeaderAdj = relationshipWithFactionLeader;
            if (relationshipWithFactionLeader < 0)
            {
                relationshipWithFactionLeaderAdj = Scores.NegativeLeaderRelationshipWeight * relationshipWithFactionLeader;
            }


            yield return new Tuple<TextObject, float>(_TRelationsFactionLeader, relationshipWithFactionLeaderAdj);

            var relationshipWithRuler = clan.GetRelationWithClan(clan.Kingdom.RulingClan);
            var relationshipWithRulerAdj = relationshipWithRuler <= 0 ? relationshipWithRuler * mercyMultiplier : relationshipWithRuler;

            yield return new Tuple<TextObject, float>(_TRelationsFactionTarget, -relationshipWithRulerAdj);
        }
    }
}
