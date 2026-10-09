namespace Diplomacy.Extensions
{
    internal static class KingdomExtensions
    {
        public static bool IsRebelKingdom(this TaleWorlds.CampaignSystem.Kingdom kingdom) => kingdom.Rebel;
        public static IEnumerable<CivilWar.Factions.RebelFaction> GetRebelFactions(this TaleWorlds.CampaignSystem.Kingdom kingdom)
            => kingdom.Factions;
    }
}
