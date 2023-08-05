using Diplomacy.CivilWar.Factions;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar.Scoring
{
    internal abstract class ChangeRulerFactionScoreBase : AbstractFactionDemandScoringModel
    {
        public float FiefDeficit => 10;

        protected static readonly TextObject _TFiefDeficit = new(StringConstants.NotEnoughFiefs);
        protected static readonly TextObject _TFriendlyClansInFaction = new("{=qAcR4Yee}Friendly Clans In Faction");
        private static readonly TextObject _TRelationsFactionTarget = new("{=dr1OCcmL}Relations with Ruler");
        protected static readonly TextObject _TLeaderTrait = new("{=EYB5Uggd}Leader is {TRAIT}");
        protected static readonly TextObject _TRulerTrait = new("{=E00ywbqb}Ruler is {TRAIT}");
        protected static readonly TextObject _TRulerNeedsRightToRule = new("{=1AZK0UVJ}Ruler doesn't have Right to Rule");

        protected ChangeRulerFactionScoreBase(IFactionDemandScores scores) : base(scores) { }

        protected Tuple<TextObject, float> CalculateFiefDeficitScore(Clan clan)
        {
            var generosityMultiplier = Math.Abs(clan.Leader.GetTraitLevel(DefaultTraits.Generosity) - DefaultTraits.Generosity.MaxValue);
            var scorePerFiefDeficit = generosityMultiplier * FiefDeficit;

            // happy with fiefs
            float fiefDeficit = clan.Tier - clan.Fiefs.Count;
            return new Tuple<TextObject, float>(_TFiefDeficit, MBMath.ClampFloat(fiefDeficit * scorePerFiefDeficit, 0, float.MaxValue));
        }

        protected IEnumerable<Tuple<TextObject, float>> CalculateTraitScore(Clan clan, RebelFaction rebelFaction, Hero hero, TraitObject trait)
        {
            var clanLeader = clan.Leader;

            var factionLeader = hero == rebelFaction.SponsorClan.Leader;

            if (clanLeader.GetTraitLevel(trait) > 0)
            {
                var heroTraitLevel = hero.GetTraitLevel(trait);
                if (heroTraitLevel != 0)
                {
                    var traitName = GameTexts.FindText("str_trait_name_" + trait.StringId.ToLower(), (heroTraitLevel + Math.Abs(trait.MinValue)).ToString());
                    var baseText = factionLeader ? _TLeaderTrait : _TRulerTrait;
                    var heroTraitText = baseText.CopyTextObject().SetTextVariable("TRAIT", traitName);
                    var score = heroTraitLevel * 10f;
                    yield return new Tuple<TextObject, float>(heroTraitText, factionLeader ? score : -score);
                }
            }
        }

        protected override Tuple<TextObject, float> GetRelationshipScoreWithTarget(Clan clan, RebelFaction rebelFaction)
        {
            if (clan?.Leader is null)
                new Tuple<TextObject, float>(_TRelationsFactionTarget, 0);

            var mercyMultiplier = 1 - (clan!.Leader!.GetTraitLevel(DefaultTraits.Mercy) * 0.25f);
            var relationshipWithRuler = clan.GetRelationWithClan(clan.Kingdom.RulingClan);
            var relationshipWithRulerAdj = relationshipWithRuler <= 0 ? relationshipWithRuler * mercyMultiplier : relationshipWithRuler;

            return new Tuple<TextObject, float>(_TRelationsFactionTarget, -relationshipWithRulerAdj);
        }
    }
}