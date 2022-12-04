using Diplomacy.CivilWar.Factions;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;

namespace Diplomacy.CivilWar.Scoring
{
    internal sealed class RebelFactionScoringModel
    {
        public const float RequiredScore = 100f;

        private static Dictionary<RebelDemandType, AbstractFactionDemandScoringModel> DemandScoreCalculators { get; } =
            new()
            {
                { RebelDemandType.Secession, new SecessionDemandScore() },
                { RebelDemandType.Abdication, new AbdicationDemandScore() }
            };

        public static Dictionary<RebelFaction, ExplainedNumber> GetDemandScore(Clan clan)
        {
            Dictionary<RebelFaction, ExplainedNumber> response = new();
            foreach (var demand in DemandScoreCalculators)
            {
                var calculatorResponse = demand.Value.GetScore(clan);
                response[calculatorResponse.Item1] = calculatorResponse.Item2;
            }

            return response;
        }

        public static ExplainedNumber GetDemandScore(Clan clan, RebelFaction rebelFaction)
        {
            return DemandScoreCalculators[rebelFaction.RebelDemandType].GetScore(clan, rebelFaction);
        }
    }
}