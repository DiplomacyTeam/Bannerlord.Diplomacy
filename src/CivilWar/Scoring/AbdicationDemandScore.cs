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

        public AbdicationDemandScore() : base(new AbdicationScores()) { }

        protected override List<RebelFaction> GetPossibleFactions(Clan clan)
        {
            return new List<RebelFaction>() { new AbdicationFaction(clan) };
        }

        protected override IEnumerable<Tuple<TextObject, float>> GetMemberScore(Clan clan, RebelFaction rebelFaction)
        {
            var generosityMultiplier = Math.Abs(clan.Leader.GetTraitLevel(DefaultTraits.Generosity) - DefaultTraits.Generosity.MaxValue);
            var scorePerFiefDeficit = generosityMultiplier * FiefDeficit;
            // happy with fiefs
            float fiefDeficit = (clan.Tier - 1 - clan.Fiefs.Count);
            if (fiefDeficit > 0)
            {
                yield return new Tuple<TextObject, float>(_TFiefDeficit, fiefDeficit * scorePerFiefDeficit);
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