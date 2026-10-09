namespace Diplomacy.CivilWar.Scoring
{
    internal static class RebelFactionScoringModel
    {
        public const float RequiredScore = 100;
        public static DemandScore GetDemandScore(TaleWorlds.CampaignSystem.Clan clan, Factions.RebelFaction faction)
            => new(clan.Support);
    }

    internal readonly record struct DemandScore(float ResultNumber);
}
