using Helpers;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.Models
{
    /// <summary>
    /// Wraps the game's alliance model so the mod's alliance settings actually do something.
    ///
    /// Vanilla (<see cref="DefaultAllianceModel"/>) only lets two kingdoms score an alliance at all
    /// when each of them has a neighbor whose threat score exceeds a hard-coded 430 — roughly a
    /// neighbor 1.2x to 1.9x stronger than they are. On a balanced map that almost never happens,
    /// which is why players report never seeing an alliance and always being told the other realm
    /// "is not seeking alliances at the moment". That gate runs before any scoring, so a score
    /// modifier alone can't change the outcome; the gate itself has to be configurable.
    ///
    /// <see cref="GetScoreOfStartingAlliance"/> therefore re-implements the vanilla scoring with
    /// <see cref="Settings.AllianceThreatThreshold"/> in place of the constant and
    /// <see cref="Settings.AllianceTendency"/> added to the result. Everything else defers to the
    /// model that was registered before ours. Mirrors DefaultAllianceModel as of game v1.4.8; the
    /// effect helpers below are copies of its private ones, so re-check them on game updates.
    /// </summary>
    public class DiplomacyAllianceModel : AllianceModel
    {
        // Vanilla's CanMakeAlliance accepts a score of 50 or more; nothing below changes that bar.
        private const float ThreatScoreCoefficient = 130f;
        private const float AllianceScoreNormalizationFactor = 0.08f;
        private const float ExposureScoreCap = 1.7f;
        private const float BaseThreatOffset = 0.4f;
        private const float MaxPowerRatio = 3f;
        private const float ThreatEffectFactor = 0.66f;
        private const float ExistingAlliancePenalty = -48f;
        private const int MaxReasonsInExplanation = 3;

        // Vanilla string ids, so they stay localized by the game.
        private const string SAllianceNotFormed = "{=Y20TbMLR}An alliance cannot be formed due to:{newline}{newline}{REASON}";
        private const string SNotConsidering = "{=tbH04aAX}{KINGDOM} is not considering an alliance with your realm due to:{newline}{newline}{REASONS_BY_LINE}";
        private const string SNotNeighbors = "{=Bu6YdMme}Kingdoms aren't neighbors.";
        private const string SNotSeekingAlliance = "{=ml9bhOka}{KINGDOM_NAME} is not seeking alliances at the moment.";
        private const string SNotPossibleAlly = "{=XnwKgWab}{KINGDOM_NAME} currently does not consider your realm to be a possible ally.";
        private const string SLowRelations = "{=3YVDMg5X}Low relations between rulers.";
        private const string SAtWarWithAlly = "{=tT91z3AL}Your realm is at war with their ally.";
        private const string SSameCultureFiefs = "{=S5lz4aHk}Your realm is occupying fiefs belonging to their culture.";
        private const string SAtWar = "{=aUStIIWw}Your realm is participating in a war.";
        private const string SLowHonor = "{=VIkVcmaE}{RULER.NAME} has low honor.";
        private const string STooManyAlliancesPlayer = "{=2RiYKRM8}Number of alliances your realm's already in: {NUMBER_OF_ALLIES}/{MAX_NUMBER_OF_ALLIES}";
        private const string STooManyAlliancesAI = "{=cFYMdTcr}Number of alliances {KINGDOM_NAME} is already in: {NUMBER_OF_ALLIES}/{MAX_NUMBER_OF_ALLIES}";

        // Mod strings.
        private const string SAllianceTendency = "{=Vb7cKfGe}Alliance tendency (Diplomacy setting)";

        private readonly AllianceModel _previousModel;

        public DiplomacyAllianceModel(AllianceModel? previousModel)
        {
            _previousModel = previousModel ?? new DefaultAllianceModel();
        }

        public override CampaignTime MaxDurationOfAlliance => _previousModel.MaxDurationOfAlliance;
        public override CampaignTime MaxDurationOfWarParticipation => _previousModel.MaxDurationOfWarParticipation;
        public override int MaxNumberOfAlliances => _previousModel.MaxNumberOfAlliances;
        public override CampaignTime DurationForOffers => _previousModel.DurationForOffers;

        public override int GetCallToWarCost(Kingdom callingKingdom, Kingdom calledKingdom, Kingdom kingdomToCallToWarAgainst)
            => _previousModel.GetCallToWarCost(callingKingdom, calledKingdom, kingdomToCallToWarAgainst);

        public override float GetSupportScoreOfStartingAllianceForClan(Kingdom kingdomDeclaresAlliance, Kingdom kingdomDeclaredAlliance, Clan evaluatingClan, out TextObject explanation, bool includeDescription = false)
            => _previousModel.GetSupportScoreOfStartingAllianceForClan(kingdomDeclaresAlliance, kingdomDeclaredAlliance, evaluatingClan, out explanation, includeDescription);

        public override float GetScoreOfCallingToWar(Kingdom callingKingdom, Kingdom calledKingdom, Kingdom kingdomToCallToWarAgainst, IFaction evaluatingFaction, out TextObject reason)
            => _previousModel.GetScoreOfCallingToWar(callingKingdom, calledKingdom, kingdomToCallToWarAgainst, evaluatingFaction, out reason);

        public override float GetScoreOfJoiningWar(Kingdom offeringKingdom, Kingdom kingdomToOfferToJoinWarWith, Kingdom kingdomToOfferToJoinWarAgainst, IFaction evaluatingFaction, out TextObject reason)
            => _previousModel.GetScoreOfJoiningWar(offeringKingdom, kingdomToOfferToJoinWarWith, kingdomToOfferToJoinWarAgainst, evaluatingFaction, out reason);

        public override int GetInfluenceCostOfProposingStartingAlliance(Clan proposingClan)
            => _previousModel.GetInfluenceCostOfProposingStartingAlliance(proposingClan);

        public override int GetInfluenceCostOfCallingToWar(Clan proposingClan)
            => _previousModel.GetInfluenceCostOfCallingToWar(proposingClan);

        public override float GetAllianceFactorForDeclaringWar(IFaction factionDeclaresWar, IFaction factionDeclaredWar)
            => _previousModel.GetAllianceFactorForDeclaringWar(factionDeclaresWar, factionDeclaredWar);

        public override float GetAllianceFactorForDeclaringPeace(IFaction factionDeclaresPeace, IFaction factionDeclaredPeace)
            => _previousModel.GetAllianceFactorForDeclaringPeace(factionDeclaresPeace, factionDeclaredPeace);

        public override Clan GetProposerClanForAllianceDecision(Kingdom proposerKingdom, Kingdom proposedKingdom)
            => _previousModel.GetProposerClanForAllianceDecision(proposerKingdom, proposedKingdom);

        public override bool CanMakeAlliance(Kingdom kingdom, Kingdom targetKingdom, IFaction evaluatingFaction, out TextObject reason, bool includeReason = false)
        {
            if (!Settings.Instance!.EnableAlliances)
            {
                reason = includeReason ? BuildNotFormedText(new TextObject(StringConstants.AlliancesDisabled)) : null!;
                return false;
            }

            // The previous model scores through Campaign.Current.Models.AllianceModel, so this still
            // lands in our GetScoreOfStartingAlliance.
            return _previousModel.CanMakeAlliance(kingdom, targetKingdom, evaluatingFaction, out reason, includeReason);
        }

        /// <summary>
        /// Scores how much <paramref name="querierKingdom"/> wants an alliance with <paramref name="queriedKingdom"/>.
        /// When the player proposes, the AI kingdom is the querier, so explanations describe the AI's view of the player.
        /// </summary>
        public override ExplainedNumber GetScoreOfStartingAlliance(Kingdom querierKingdom, Kingdom queriedKingdom, out TextObject explanation, bool includeDescription = false)
        {
            var result = new ExplainedNumber(0f, includeDescription);

            if (!PassesAllianceGate(querierKingdom, queriedKingdom, includeDescription, out var threats, out var rejection))
            {
                explanation = BuildNotFormedText(rejection);
                return result;
            }

            var negativeReasons = new List<(float score, TextObject text)>();
            foreach (var (score, text, explainWhenNegative) in GetScoreEffects(querierKingdom, queriedKingdom, threats, includeDescription))
            {
                result.Add(score, text);
                if (explainWhenNegative && includeDescription && score < 0f)
                    negativeReasons.Add((score, text));
            }

            explanation = negativeReasons.Count > 0
                ? BuildNotConsideringText(querierKingdom, negativeReasons)
                : BuildNotFormedText(includeDescription ? BuildNotPossibleAllyText(querierKingdom) : null);

            return result;
        }

        private readonly struct Threats
        {
            public readonly Kingdom? QuerierThreat;
            public readonly float QuerierThreatScore;
            public readonly Kingdom? QueriedThreat;
            public readonly float QueriedThreatScore;

            public Threats(Kingdom? querierThreat, float querierThreatScore, Kingdom? queriedThreat, float queriedThreatScore)
            {
                QuerierThreat = querierThreat;
                QuerierThreatScore = querierThreatScore;
                QueriedThreat = queriedThreat;
                QueriedThreatScore = queriedThreatScore;
            }
        }

        /// <summary>
        /// Vanilla's precondition for scoring at all: the kingdoms must border each other and each must
        /// have a neighbor more threatening than <see cref="Settings.AllianceThreatThreshold"/>.
        /// <paramref name="rejection"/> is only built when <paramref name="includeDescription"/> is set.
        /// </summary>
        private static bool PassesAllianceGate(Kingdom querierKingdom, Kingdom queriedKingdom, bool includeDescription, out Threats threats, out TextObject? rejection)
        {
            threats = default;
            rejection = null;

            if (!Settings.Instance!.EnableAlliances)
            {
                rejection = includeDescription ? new TextObject(StringConstants.AlliancesDisabled) : null;
                return false;
            }

            if (!AreKingdomsNeighbors(querierKingdom, queriedKingdom))
            {
                rejection = includeDescription ? new TextObject(SNotNeighbors) : null;
                return false;
            }

            float threatThreshold = Settings.Instance.AllianceThreatThreshold;

            var (querierThreat, querierThreatScore) = GetThreateningNeighbor(querierKingdom);
            if (querierThreat is null || querierThreatScore <= threatThreshold || querierThreat == queriedKingdom)
            {
                rejection = includeDescription ? BuildNotSeekingText(querierKingdom) : null;
                return false;
            }

            var (queriedThreat, queriedThreatScore) = GetThreateningNeighbor(queriedKingdom);
            if (queriedThreatScore <= threatThreshold)
            {
                rejection = includeDescription ? BuildNotPossibleAllyText(querierKingdom) : null;
                return false;
            }

            threats = new Threats(querierThreat, querierThreatScore, queriedThreat, queriedThreatScore);
            return true;
        }

        /// <summary>
        /// Every term of the alliance score, in vanilla's order, plus the mod's tendency at the end.
        /// The flag marks terms whose negative values are worth showing the player as a reason.
        /// </summary>
        private IEnumerable<(float score, TextObject text, bool explainWhenNegative)> GetScoreEffects(Kingdom querierKingdom, Kingdom queriedKingdom, Threats threats, bool includeDescription)
        {
            yield return (GetAlliancePenalty(querierKingdom), GetAlliancePenaltyText(querierKingdom, includeDescription), true);
            yield return (GetAlliancePenalty(queriedKingdom), GetAlliancePenaltyText(queriedKingdom, includeDescription), true);
            yield return (GetThreatEffect(threats.QueriedThreatScore, threats.QuerierThreatScore), new TextObject("{=!}Threat Effect"), false);
            yield return (GetRelationshipEffect(querierKingdom, queriedKingdom), new TextObject(SLowRelations), true);
            yield return (GetMarriageEffect(querierKingdom, queriedKingdom), new TextObject("{=!}Marriage Effect"), false);
            yield return (GetAtWarWithAllyEffect(querierKingdom, queriedKingdom), new TextObject(SAtWarWithAlly), true);
            yield return (GetAtWarWithEnemyEffect(querierKingdom, queriedKingdom), new TextObject("{=!}At war with enemy Effect"), false);
            yield return (GetAtWarOrPeaceEffect(queriedKingdom), new TextObject(SAtWar), true);
            yield return (GetFiefWithSameCultureEffect(querierKingdom, queriedKingdom), new TextObject(SSameCultureFiefs), true);
            yield return (GetHonorableKingEffect(querierKingdom, queriedKingdom), BuildLowHonorText(querierKingdom, includeDescription), true);
            yield return (GetTradeAgreementEffect(querierKingdom, queriedKingdom), new TextObject("{=!}Trade Agreement Effect"), false);
            yield return (GetCommonThreatEffect(threats.QuerierThreat, threats.QueriedThreat), new TextObject("{=!}Common Threat Effect"), false);

            // The mod's own contribution. Applied only once vanilla's gate has passed, so a high
            // tendency can't conjure alliances between realms that aren't even neighbors.
            yield return (Settings.Instance!.AllianceTendency, new TextObject(SAllianceTendency), false);
        }

        private static TextObject BuildLowHonorText(Kingdom querierKingdom, bool includeDescription)
        {
            var text = new TextObject(SLowHonor);
            if (includeDescription && querierKingdom.Leader is not null)
                StringHelpers.SetCharacterProperties("RULER", querierKingdom.Leader.CharacterObject, text);
            return text;
        }

        private static TextObject BuildNotSeekingText(Kingdom querierKingdom)
        {
            var text = new TextObject(SNotSeekingAlliance);
            text.SetTextVariable("KINGDOM_NAME", querierKingdom.Name);
            return text;
        }

        private static TextObject BuildNotFormedText(TextObject? reason)
        {
            var text = new TextObject(SAllianceNotFormed);
            if (reason is not null)
                text.SetTextVariable("REASON", reason);
            return text;
        }

        private static TextObject BuildNotPossibleAllyText(Kingdom querierKingdom)
        {
            var text = new TextObject(SNotPossibleAlly);
            text.SetTextVariable("KINGDOM_NAME", querierKingdom.Name);
            return text;
        }

        private static TextObject BuildNotConsideringText(Kingdom querierKingdom, List<(float score, TextObject text)> negativeReasons)
        {
            var reasons = negativeReasons.OrderBy(r => r.score).Take(MaxReasonsInExplanation).Select(r => r.text).ToList();

            var format = string.Join("{newline}", Enumerable.Range(0, reasons.Count).Select(i => $"{{REASON_{i}}}"));
            var reasonsByLine = new TextObject("{=!}" + format);
            for (var i = 0; i < reasons.Count; i++)
                reasonsByLine.SetTextVariable($"REASON_{i}", reasons[i]);

            var text = new TextObject(SNotConsidering);
            text.SetTextVariable("KINGDOM", querierKingdom.Name);
            text.SetTextVariable("REASONS_BY_LINE", reasonsByLine);
            return text;
        }

        #region Vanilla helpers (DefaultAllianceModel, game v1.4.8)

        private static bool AreKingdomsNeighbors(Kingdom kingdom1, Kingdom kingdom2)
        {
            if (kingdom1 == kingdom2 || kingdom1.Fiefs.Count == 0 || kingdom2.Fiefs.Count == 0)
                return false;

            foreach (var fief in kingdom1.Fiefs)
            {
                foreach (var neighbor in fief.GetNeighborFortifications(MobileParty.NavigationType.All))
                {
                    if (neighbor.MapFaction == kingdom2)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The neighboring kingdom with the highest threat score, where threat grows with how much of the
        /// querier's border that neighbor covers and how much stronger it is.
        /// </summary>
        private static (Kingdom? kingdom, float threatScore) GetThreateningNeighbor(Kingdom querierKingdom)
        {
            var countedFortifications = new HashSet<Settlement>();
            var borderShare = new Dictionary<Kingdom, float>();
            var totalBorder = 0f;

            foreach (var fief in querierKingdom.Fiefs)
            {
                foreach (var neighbor in fief.GetNeighborFortifications(MobileParty.NavigationType.All))
                {
                    if (neighbor.MapFaction == querierKingdom || !neighbor.MapFaction.IsKingdomFaction || countedFortifications.Contains(neighbor))
                        continue;

                    var neighborKingdom = (Kingdom) neighbor.MapFaction;
                    borderShare[neighborKingdom] = borderShare.TryGetValue(neighborKingdom, out var count) ? count + 1f : 1f;
                    totalBorder++;
                    countedFortifications.Add(neighbor);
                }
            }

            Kingdom? mostThreatening = null;
            var highestThreat = 0f;
            var querierStrength = CalculateKingdomStrength(querierKingdom);

            foreach (var pair in borderShare)
            {
                var threat = CalculateThreatScore(pair.Value, totalBorder, CalculateKingdomStrength(pair.Key), querierStrength);
                if (highestThreat < threat)
                {
                    mostThreatening = pair.Key;
                    highestThreat = threat;
                }
            }

            return (mostThreatening, highestThreat);
        }

        private static float CalculateThreatScore(float neighborScore, float totalNeighborScore, float powerOfThreat, float powerOfQuerier)
        {
            if (powerOfQuerier <= 0f || totalNeighborScore <= 0f)
                return 0f;

            var exposureScore = MBMath.Map(neighborScore / totalNeighborScore, 0f, 1f, 1f, 2f);
            var powerRatio = MathF.Clamp(powerOfThreat / powerOfQuerier, 0f, MaxPowerRatio);
            return (MathF.Min(exposureScore, ExposureScoreCap) + BaseThreatOffset + powerRatio) * ThreatScoreCoefficient;
        }

        private static float CalculateKingdomStrength(Kingdom kingdom)
            => kingdom.Clans.Where(c => !c.IsUnderMercenaryService).Sum(c => c.CurrentTotalStrength);

        private static float GetThreatEffect(float threatScoreForQuerier, float threatScoreForQueried)
            => AllianceScoreNormalizationFactor * (threatScoreForQueried + threatScoreForQuerier) * ThreatEffectFactor;

        private static float GetRelationshipEffect(Kingdom querierKingdom, Kingdom queriedKingdom)
        {
            if (querierKingdom.Leader is null || queriedKingdom.Leader is null)
                return 0f;

            var relation = querierKingdom.Leader.GetRelation(queriedKingdom.Leader);
            var calculating = querierKingdom.Leader.GetTraitLevel(DefaultTraits.Calculating);
            if (relation > 0 || calculating <= 0)
                return MathF.Clamp(relation, -100f, 100f) * AllianceScoreNormalizationFactor;

            return 0f;
        }

        private static float GetMarriageEffect(Kingdom querierKingdom, Kingdom queriedKingdom)
        {
            if (IsThereMarriageBetweenClans(querierKingdom.RulingClan, queriedKingdom.RulingClan))
                return 8f;

            if (querierKingdom.Leader is not null && (queriedKingdom.RulingClan?.AliveLords.Any(x => x.Spouse?.Father?.MapFaction == queriedKingdom) ?? false))
                return 4f;

            return 0f;
        }

        private static bool IsThereMarriageBetweenClans(Clan? clan1, Clan? clan2)
        {
            if (clan1 is null || clan2 is null)
                return false;

            return clan1.AliveLords.Any(x => x.Spouse?.Father?.Clan == clan2)
                || clan2.AliveLords.Any(x => x.Spouse?.Father?.Clan == clan1);
        }

        private static float GetAtWarWithAllyEffect(Kingdom querierKingdom, Kingdom queriedKingdom)
            => (queriedKingdom.FactionsAtWarWith.Any(x => x.IsKingdomFaction && querierKingdom.IsAllyWith((Kingdom) x)) ? -100f : 0f) * AllianceScoreNormalizationFactor;

        private static float GetAtWarWithEnemyEffect(Kingdom querierKingdom, Kingdom queriedKingdom)
            => (queriedKingdom.FactionsAtWarWith.Any(x => x.IsKingdomFaction && querierKingdom.IsAtWarWith((Kingdom) x)) ? 100f : 0f) * AllianceScoreNormalizationFactor;

        private static float GetAtWarOrPeaceEffect(Kingdom queriedKingdom)
            => (queriedKingdom.FactionsAtWarWith.Any(x => x.IsKingdomFaction) ? -5f : 25f) * AllianceScoreNormalizationFactor;

        private static float GetFiefWithSameCultureEffect(Kingdom querierKingdom, Kingdom queriedKingdom)
        {
            float fiefCount = queriedKingdom.Fiefs.Count;
            if (fiefCount <= 0f)
                return 0f;

            var sameCultureFiefs = queriedKingdom.Fiefs.Count(fief => fief.Culture == querierKingdom.Culture);
            return MathF.Clamp(sameCultureFiefs / fiefCount * -200f, -200f, 0f) * AllianceScoreNormalizationFactor;
        }

        private static float GetHonorableKingEffect(Kingdom querierKingdom, Kingdom queriedKingdom)
        {
            if (querierKingdom.Leader is null)
                return 0f;

            var querierHonor = querierKingdom.Leader.GetTraitLevel(DefaultTraits.Honor);
            var queriedHonor = queriedKingdom.Leader?.GetTraitLevel(DefaultTraits.Honor) ?? 0;
            if (querierHonor > 0)
                return 4f;
            if (querierHonor < 0 && queriedHonor > 0)
                return -4f;

            return 0f;
        }

        private static float GetTradeAgreementEffect(Kingdom querierKingdom, Kingdom queriedKingdom)
        {
            var tradeAgreements = Campaign.Current?.GetCampaignBehavior<ITradeAgreementsCampaignBehavior>();
            return tradeAgreements is not null && tradeAgreements.HasTradeAgreement(querierKingdom, queriedKingdom, out _) ? 4f : 0f;
        }

        private static float GetCommonThreatEffect(Kingdom? threatForQuerier, Kingdom? threatForQueried)
            => threatForQuerier is not null && threatForQuerier == threatForQueried ? 8f : 0f;

        private float GetAlliancePenalty(Kingdom kingdom)
        {
            if (kingdom.AlliedKingdoms.Count <= 0)
                return 0f;

            var penalty = ExistingAlliancePenalty;
            if (kingdom == Clan.PlayerClan.Kingdom)
                penalty *= 0.5f;
            return penalty;
        }

        private TextObject GetAlliancePenaltyText(Kingdom kingdom, bool includeDescription)
        {
            var text = new TextObject(kingdom == Clan.PlayerClan.Kingdom ? STooManyAlliancesPlayer : STooManyAlliancesAI);
            if (includeDescription)
            {
                text.SetTextVariable("NUMBER_OF_ALLIES", kingdom.AlliedKingdoms.Count);
                text.SetTextVariable("KINGDOM_NAME", kingdom.Name);
                text.SetTextVariable("MAX_NUMBER_OF_ALLIES", MaxNumberOfAlliances);
            }
            return text;
        }

        #endregion
    }
}
