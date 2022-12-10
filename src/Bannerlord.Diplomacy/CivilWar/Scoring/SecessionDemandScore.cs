using Diplomacy.CivilWar.Factions;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar.Scoring
{
    class SecessionDemandScore : ChangeRulerFactionScoreBase
    {
        private static readonly TextObject _TCalculating = new("{=Jc1mCVuY}Trait: Calculating");
        private static readonly TextObject _TClanTier = new("{=cjbVV7E3}Clan Tier Too Low");
        private static readonly TextObject _TRightToRule = new("{=IYO5TNTg}Right to Rule");
        private static readonly TextObject _TLeaderRightToRule = new("{=YLUocN5e}Leader has Right to Rule");

        public SecessionDemandScore() : base(new SecessionScores()) { }

        protected override List<RebelFaction> GetPossibleFactions(Clan clan)
        {
            return new() { new SecessionFaction(clan) };
        }

        protected override IEnumerable<Tuple<TextObject, float>> GetMemberScore(Clan clan, RebelFaction rebelFaction)
        {
            List<Tuple<TextObject, float>> memberScores = new() { CalculateFiefDeficitScore(clan) };
            memberScores.AddRange(CalculateTraitScore(clan, rebelFaction, rebelFaction.ParentKingdom.Leader, DefaultTraits.Honor));
            memberScores.AddRange(CalculateTraitScore(clan, rebelFaction, rebelFaction.ParentKingdom.Leader, DefaultTraits.Valor));

            if (rebelFaction.ParentKingdom.Leader.Clan.Tier < 6)
            {
                memberScores.Add(new Tuple<TextObject, float>(_TRulerNeedsRightToRule, 25f));
            }

            return memberScores;
        }

        protected override IEnumerable<Tuple<TextObject, float>> GetLeaderOnlyScore(Clan clan, RebelFaction rebelFaction)
        {
            // ambition of faction sponsor
            yield return new Tuple<TextObject, float>(_TCalculating, clan.Leader.GetTraitLevel(DefaultTraits.Calculating) * 20);

            // must be clan tier 4+
            if (clan.Tier < 4)
            {
                yield return new Tuple<TextObject, float>(_TClanTier, -999f);
            }

            if (clan.Tier >= 6)
            {
                yield return new Tuple<TextObject, float>(_TRightToRule, 50f);
            }
        }

        protected override IEnumerable<Tuple<TextObject, float>> GetMemberOnlyScore(Clan clan, RebelFaction rebelFaction)
        {
            if (rebelFaction.SponsorClan.Tier >= 6)
            {
                yield return new Tuple<TextObject, float>(_TLeaderRightToRule, 25f);
            }

            var friendlyClans = rebelFaction.Clans.Where(x => x != clan && x != rebelFaction.SponsorClan && clan.Leader.IsFriend(x.Leader));

            yield return new Tuple<TextObject, float>(_TFriendlyClansInFaction, friendlyClans.Count() * 5f);

            var honorTrait = CalculateTraitScore(clan, rebelFaction, rebelFaction.SponsorClan.Leader, DefaultTraits.Honor);
            var valorTrait = CalculateTraitScore(clan, rebelFaction, rebelFaction.SponsorClan.Leader, DefaultTraits.Valor);

            var traits = honorTrait.Concat(valorTrait);
            foreach (var tuple in traits)
            {
                yield return tuple;
            }
        }

        public class SecessionScores : IFactionDemandScores
        {
            public float Base => 10;
            public float NegativeLeaderRelationshipWeight => 1;
            public float KingdomSizeScoreMax => 50;
            public float KingdomSizeCastleScore => 1;
            public float KingdomSizeTownScore => 2;
        }
    }
}