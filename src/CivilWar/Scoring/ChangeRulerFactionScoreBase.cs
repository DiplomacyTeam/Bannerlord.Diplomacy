using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar
{

    internal abstract class ChangeRulerFactionScoreBase : AbstractFactionDemandScoringModel
    {
        public float FiefDeficit => 10;

        protected static readonly TextObject _TFiefDeficit = new TextObject(StringConstants.NotEnoughFiefs);
        protected static readonly TextObject _TFriendlyClansInFaction = new TextObject("{=qAcR4Yee}Friendly Clans In Faction");
        private static readonly TextObject _TRelationsFactionTarget = new TextObject("{=dr1OCcmL}Relations with Ruler");
        protected static readonly TextObject _TLeaderTrait = new TextObject("{=EYB5Uggd}Leader is {TRAIT}");
        protected static readonly TextObject _TRulerTrait = new TextObject("{=E00ywbqb}Ruler is {TRAIT}");


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
            Hero clanLeader = clan.Leader;

            bool factionLeader = hero == rebelFaction.SponsorClan.Leader;

            if (clanLeader.GetTraitLevel(trait) > 0)
            {
                int heroTraitLevel = hero.GetTraitLevel(trait);
                if (heroTraitLevel != 0)
                {
                    TextObject traitName = GameTexts.FindText("str_trait_name_" + trait.StringId.ToLower(), (heroTraitLevel + Math.Abs(trait.MinValue)).ToString());
                    TextObject baseText = factionLeader ? _TLeaderTrait : _TRulerTrait;
                    TextObject heroTraitText = baseText.CopyTextObject().SetTextVariable("TRAIT", traitName);
                    float score = heroTraitLevel * 10f;
                    yield return new Tuple<TextObject, float>(heroTraitText, factionLeader ? score : -score);
                }
            }
        }

        protected override Tuple<TextObject, float> GetRelationshipScoreWithTarget(Clan clan, RebelFaction rebelFaction)
        {
            var mercyMultiplier = 1 - (clan.Leader.GetTraitLevel(DefaultTraits.Mercy) * 0.25f);
            var relationshipWithRuler = clan.GetRelationWithClan(clan.Kingdom.RulingClan);
            var relationshipWithRulerAdj = relationshipWithRuler <= 0 ? relationshipWithRuler * mercyMultiplier : relationshipWithRuler;

            return new Tuple<TextObject, float>(_TRelationsFactionTarget, -relationshipWithRulerAdj);
        }
    }
}