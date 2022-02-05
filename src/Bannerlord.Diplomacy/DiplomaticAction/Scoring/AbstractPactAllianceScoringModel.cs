using Diplomacy.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Barterables;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Scoring
{
    internal abstract class AbstractPactAllianceScoringModel<T> : AbstractAgreementScoringModel<T> where T : AbstractPactAllianceScoringModel<T>, new() 
    {

        public abstract IDiplomacyScores Scores { get; }

        public override List<ScoreEvaluator> ScoreEvaluators => new() { BaselineEvaluator, ThirdPartyDiplomaticRelationsEvaluator, TreatiesOverburdenEvaluator, RelationsEvaluator, ExpansionismEvaluator };

        public override float BaseScore => Scores.Base;

        protected virtual void BaselineEvaluator(Kingdom ourKingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType, ref ExplainedNumber explainedNumber)
        {
            // Weak Kingdom (Us)
            if (!ourKingdom.IsStrong())
                explainedNumber.Add(Scores.BelowMedianStrength, _TWeakKingdom);

            //Offer appreciation (Us)
            if (kingdomPartyType == DiplomaticPartyType.Recipient)
                explainedNumber.Add(Scores.AppreciatesTheOffer, _TOfferAppreciation);

            // Tendency
            explainedNumber.Add(Scores.Tendency, _TTendency);
        }

        protected virtual void ThirdPartyDiplomaticRelationsEvaluator(Kingdom ourKingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType, ref ExplainedNumber explainedNumber)
        {
            // Common Enemies
            var commonEnemies = FactionManager.GetEnemyKingdoms(ourKingdom).Intersect(FactionManager.GetEnemyKingdoms(otherKingdom));
            //TODO: add [0.0f ; 1.0f] multiplier based on how close is each individual war to an end. Supposedly.
            foreach (var commonEnemy in commonEnemies)
                explainedNumber.Add(Scores.HasCommonEnemy, CreateTextWithKingdom(SCommonEnemy, commonEnemy));

            // Potential Enemies (Them)
            if (Scores.HasPotentialEnemy != 0)
            {
                var potentialEnemies = FactionManager.GetEnemyKingdoms(otherKingdom).Except(FactionManager.GetEnemyKingdoms(ourKingdom));
                foreach (var potentialEnemy in potentialEnemies.ToList())
                {
                    var multiplier = GetNewEnemyMultiplier(ourKingdom, potentialEnemy);
                    if (multiplier != 0)
                        explainedNumber.Add((int) (Scores.HasPotentialEnemy * multiplier), CreateTextWithKingdom(SUnwantedEnemy, potentialEnemy));
                }
            }

            // Their Alliances with Enemies
            var alliedEnemies = Kingdom.All
                .Where(k => k != ourKingdom
                         && k != otherKingdom
                         && FactionManager.IsAlliedWithFaction(otherKingdom, k)
                         && FactionManager.IsAtWarAgainstFaction(ourKingdom, k));

            foreach (var alliedEnemy in alliedEnemies)
                explainedNumber.Add(Scores.ExistingAllianceWithEnemy, CreateTextWithKingdom(SAlliedToEnemy, alliedEnemy));

            // Their Alliances with Neutrals
            var alliedNeutrals = Kingdom.All
                .Where(k => k != ourKingdom
                         && k != otherKingdom
                         && FactionManager.IsAlliedWithFaction(otherKingdom, k)
                         && !FactionManager.IsAtWarAgainstFaction(ourKingdom, k)
                         && !FactionManager.IsAlliedWithFaction(ourKingdom, k));

            foreach (var alliedNeutral in alliedNeutrals)
                explainedNumber.Add(Scores.ExistingAllianceWithNeutral, CreateTextWithKingdom(SAlliedToNeutral, alliedNeutral));

            // Their Pacts with Enemies
            var pactEnemies = Kingdom.All
                .Where(k => k != ourKingdom
                         && k != otherKingdom
                         && DiplomaticAgreementManager.HasNonAggressionPact(otherKingdom, k, out _)
                         && FactionManager.IsAtWarAgainstFaction(ourKingdom, k));

            foreach (var pactEnemy in pactEnemies)
                explainedNumber.Add(Scores.NonAggressionPactWithEnemy, CreateTextWithKingdom(SPactWithEnemy, pactEnemy));

            // Their Pacts with Neutral
            var pactNeutrals = Kingdom.All
                .Where(k => k != ourKingdom
                         && k != otherKingdom
                         && DiplomaticAgreementManager.HasNonAggressionPact(otherKingdom, k, out _)
                         && !FactionManager.IsAtWarAgainstFaction(ourKingdom, k)
                         && !FactionManager.IsAlliedWithFaction(ourKingdom, k));

            foreach (var pactNeutral in pactNeutrals)
                explainedNumber.Add(Scores.NonAggressionPactWithNeutral, CreateTextWithKingdom(SPactWithNeutral, pactNeutral));

            // Their Pacts with Allies
            var pactAllies = Kingdom.All
                .Where(k => k != ourKingdom
                         && k != otherKingdom
                         && DiplomaticAgreementManager.HasNonAggressionPact(otherKingdom, k, out _)
                         && FactionManager.IsAlliedWithFaction(ourKingdom, k));

            foreach (var pactAlly in pactAllies)
                explainedNumber.Add(Scores.NonAggressionPactWithAlly, CreateTextWithKingdom(SPactWithAlly, pactAlly));
        }

        protected virtual void TreatiesOverburdenEvaluator(Kingdom ourKingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType, ref ExplainedNumber explainedNumber)
        {
            // Too many diplomatic treaties (Us + Them)
            var treatiesOverburdenPenalty = (GetTreatiesOverburden(ourKingdom) + GetTreatiesOverburden(otherKingdom)) * Scores.TreatiesOverburden;
            if (treatiesOverburdenPenalty < 0)
                explainedNumber.Add(treatiesOverburdenPenalty, _TTreatiesOverburden);
        }

        protected virtual void RelationsEvaluator(Kingdom ourKingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType, ref ExplainedNumber explainedNumber)
        {
            // Leaders Relationship
            var relationMult = NormalizeSmootly(ourKingdom.Leader.GetRelation(otherKingdom.Leader), 100);
            explainedNumber.Add((int)(Scores.LeadersRelationship * relationMult), _TLeadersRelationship);

            //Public Relations
            var publicRelationMult = NormalizeSmootly(GetPublicRelations(ourKingdom, otherKingdom), 100);
            explainedNumber.Add((int)(Scores.PublicRelations * publicRelationMult), _TPublicRelations);
        }

        protected virtual void ExpansionismEvaluator(Kingdom ourKingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType, ref ExplainedNumber explainedNumber)
        {
            // Expansionism (Them)
            var expansionismPenalty = otherKingdom.GetExpansionismDiplomaticPenalty();
            if (expansionismPenalty < 0)
                explainedNumber.Add((int)expansionismPenalty, _TExpansionism);
        }

        public virtual bool ShouldFormBidirectional(Kingdom ourKingdom, Kingdom otherKingdom) => ShouldForm(ourKingdom, otherKingdom) && ShouldForm(otherKingdom, ourKingdom, DiplomaticPartyType.Recipient);

        public virtual bool ShouldForm(Kingdom ourKingdom, Kingdom otherKingdom, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer) => GetScore(ourKingdom, otherKingdom, kingdomPartyType).ResultNumber >= AcceptOrProposeThreshold;

        protected virtual int GetTreatiesOverburden(Kingdom kingdom) =>
            Math.Max(Kingdom.All.Count(k => k != kingdom && FactionManager.IsAlliedWithFaction(kingdom, k)) - 1, 0)
            + Math.Max(Kingdom.All.Count(k => k != kingdom && !FactionManager.IsAlliedWithFaction(kingdom, k) && DiplomaticAgreementManager.HasNonAggressionPact(kingdom, k, out _)) - 1, 0);

        protected virtual float GetNewEnemyMultiplier(Kingdom ourKingdom, Kingdom potentialEnemyKingdom)
        {
            var warValue = new DeclareWarBarterable(ourKingdom, potentialEnemyKingdom).GetValueForFaction(ourKingdom.RulingClan);
            int allianceBrokenMultiplier = 0;
            if (FactionManager.IsAlliedWithFaction(ourKingdom, potentialEnemyKingdom))
            {
                allianceBrokenMultiplier = 1;
                Kingdom.All.Where(k => k != ourKingdom && k != potentialEnemyKingdom && FactionManager.IsAlliedWithFaction(k, potentialEnemyKingdom) && FactionManager.IsAlliedWithFaction(k, ourKingdom)).ToList()
                           .ForEach(k => allianceBrokenMultiplier += 1);
            }
            return NormalizeSmootly(warValue, 500000.0f) - allianceBrokenMultiplier;
        }

        protected virtual float GetPublicRelations(Kingdom ourKingdom, Kingdom otherKingdom)
        {
            var ourClans = ourKingdom.Clans.Where(clan => clan != ourKingdom.RulingClan);
            var theirClans = otherKingdom.Clans.Where(clan => clan != otherKingdom.RulingClan);
            return (float)ourClans.SelectMany(ourClan => theirClans.Select(theirClan => (ourClan, theirClan))).DefaultIfEmpty().Average(pair => pair != default ? pair.ourClan.GetRelationWithClan(pair.theirClan) : 0);
        }

        protected float NormalizeSmootly(float originalValue, float originalValueMax, float normalizedValueMax = 1.0f) =>
            normalizedValueMax * (float)Math.Sin(Math.PI * MBMath.ClampFloat(originalValue / (originalValueMax * 2), -0.5f, +0.5f));

        protected float NormalizeLinearly(float originalValue, float originalValueMax, float normalizedValueMax = 1.0f) => normalizedValueMax * MBMath.ClampFloat(originalValue / originalValueMax, -1.0f, +1.0f);

        private static TextObject? CreateTextWithKingdom(string text, Kingdom kingdom) => new TextObject(text).SetTextVariable("KINGDOM", kingdom.Name);

        public interface IDiplomacyScores
        {
            public int Base { get; }
            public int BelowMedianStrength { get; } //Positive score
            public int HasCommonEnemy { get; } //Positive score
            public int HasPotentialEnemy { get; } //Double-edged score
            public int TreatiesOverburden { get; } //Negative score
            public int ExistingAllianceWithEnemy { get; } //Negative score
            public int ExistingAllianceWithNeutral { get; } //Negative score            
            public int NonAggressionPactWithEnemy { get; } //Negative score
            public int NonAggressionPactWithNeutral { get; } //Negative score
            public int NonAggressionPactWithAlly { get; } //Positive score
            public int LeadersRelationship { get; } //Double-edged score
            public int PublicRelations { get; } //Double-edged score
            public int AppreciatesTheOffer { get; } //Positive score
            //public int Culture { get; } //Double-edged score
            //public int Distance { get; } //Double-edged score
            //public int IsDeterrentNeighbor { get; } //Negative score
            //public int ReliabilityRating { get; } //Double-edged score
            public int Tendency { get; }
        }

        //Generic scores. Use them directly or keep just for reference?
        protected const int MinorFactor = 5;
        protected const int NominalFactor = 10;
        protected const int ConsiderableFactor = 15;
        protected const int SignificantFactor = 20;
        protected const int MajorFactor = 30;
        protected const int EssentialFactor = 50;

        private static readonly TextObject _TWeakKingdom = new("{=q5qphBwi}Weak Kingdom");
        private static readonly TextObject _TTreatiesOverburden = new("{=zXbesqx7}Too many diplomatic treaties");
        private static readonly TextObject _TLeadersRelationship = new("{=sygtLRqA}Leaders relationship");
        private static readonly TextObject _TPublicRelations = new("{=gVtFqWfP}Public relations");
        private static readonly TextObject _TExpansionism = new("{=CxdpR6w4}Expansionism");
        private static readonly TextObject _TTendency = new("{=lhysZl9j}Action Tendency");
        private static readonly TextObject _TOfferAppreciation = new("{=A06TcNG4}Appreciates the offer");

        private const string SWarWithKingdom = "{=RqQ4oqvl}War with {KINGDOM}";
        private const string SAllianceWithKingdom = "{=cmOSpfyW}Alliance with {KINGDOM}";
        private const string SPactWithKingdom = "{=t6YhBLj7}Non-Aggression Pact with {KINGDOM}";

        private const string SCommonEnemy = SWarWithKingdom;
        private const string SUnwantedEnemy = SWarWithKingdom;
        private const string SAlliedToEnemy = SAllianceWithKingdom;
        private const string SAlliedToNeutral = SAllianceWithKingdom;

        private const string SPactWithEnemy = SPactWithKingdom;
        private const string SPactWithNeutral = SPactWithKingdom;
        private const string SPactWithAlly = SPactWithKingdom;
    }
}