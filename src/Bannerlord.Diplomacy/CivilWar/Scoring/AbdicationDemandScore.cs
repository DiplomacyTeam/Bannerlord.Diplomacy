using Diplomacy.CivilWar.Factions;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar.Scoring
{
    internal sealed class AbdicationDemandScore : ChangeRulerFactionScoreBase
    {
        public AbdicationDemandScore() : base(new AbdicationScores()) { }

        protected override List<RebelFaction> GetPossibleFactions(Clan clan)
        {
            return new() { new AbdicationFaction(clan) };
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