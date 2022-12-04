using Diplomacy.CivilWar.Factions;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar.Scoring
{
    internal abstract class AbstractFactionDemandScoringModel
    {
        public IFactionDemandScores Scores { get; init; }
        protected AbstractFactionDemandScoringModel(IFactionDemandScores scores) => Scores = scores;

        private static readonly TextObject _TFactionTendency = new("{=Vp9EcnuC}Faction Tendency");
        private static readonly TextObject _TRelationsFactionLeader = new("{=mouJZAAy}Relations with Leader");
        private static readonly TextObject _TKingdomSize = new("{=FtX4LPXF}Kingdom Size");

        public ExplainedNumber GetScore(Clan clan, RebelFaction rebelFaction)
        {
            List<Tuple<TextObject, float>> scores = new();
            scores.AddRange(GetMemberScore(clan, rebelFaction));

            scores.AddRange(rebelFaction.SponsorClan == clan ? GetLeaderOnlyScore(clan, rebelFaction) : GetMemberOnlyScore(clan, rebelFaction));
            scores.AddRange(GetBaseScore(clan, rebelFaction));

            var score = new ExplainedNumber(Scores.Base, true);

            foreach (var scoreTuple in scores)
            {
                score.Add(scoreTuple.Item2, scoreTuple.Item1);
            }

            score.Add(Settings.Instance!.FactionTendency, _TFactionTendency);

            return score;
        }

        public Tuple<RebelFaction, ExplainedNumber> GetScore(Clan clan)
        {
            Dictionary<RebelFaction, ExplainedNumber> possibilityScores = new();
            foreach (var faction in GetPossibleFactions(clan))
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
            foreach (var relationship in relationships)
                yield return relationship;

            // valor modifier
            int[] valorModifier = { -20, -10, 0, 10, 20 };
            var trait = DefaultTraits.Valor;
            var traitLevel = clan.Leader.GetTraitLevel(trait);
            var valorText = GetTraitText(trait, traitLevel);
            var normalizedTraitLevel = traitLevel + Math.Abs(trait.MinValue);
            var valorScore = valorModifier[normalizedTraitLevel];
            yield return new Tuple<TextObject, float>(valorText, valorScore);

            // kingdom size
            var kingdomCastles = kingdom.Fiefs.Count(x => x.IsCastle);
            var kingdomTowns = kingdom.Fiefs.Count(x => x.IsTown);

            var kingdomSize = kingdomCastles * Scores.KingdomSizeCastleScore + kingdomTowns * Scores.KingdomSizeTownScore;
            var kingdomSizeScore = MBMath.ClampFloat(kingdomSize, 0, Scores.KingdomSizeScoreMax);

            yield return new Tuple<TextObject, float>(_TKingdomSize, kingdomSizeScore);
        }

        private static TextObject GetTraitText(TraitObject trait, int traitLevel)
        {
            return GameTexts.FindText("str_trait_name_" + trait.StringId.ToLower(), (traitLevel + Math.Abs(trait.MinValue)).ToString());
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
            var relationshipWithFactionLeader = clan != rebelFaction.SponsorClan ? clan.GetRelationWithClan(rebelFaction.SponsorClan) : 0f;

            var relationshipWithFactionLeaderAdj = relationshipWithFactionLeader;
            if (relationshipWithFactionLeader < 0)
            {
                relationshipWithFactionLeaderAdj = Scores.NegativeLeaderRelationshipWeight * relationshipWithFactionLeader;
            }

            yield return new Tuple<TextObject, float>(_TRelationsFactionLeader, relationshipWithFactionLeaderAdj);
            yield return GetRelationshipScoreWithTarget(clan, rebelFaction);
        }

        protected abstract Tuple<TextObject, float> GetRelationshipScoreWithTarget(Clan clan, RebelFaction rebelFaction);
    }
}