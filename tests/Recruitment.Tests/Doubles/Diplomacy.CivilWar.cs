namespace Diplomacy.CivilWar
{
    internal static class RebelFactionManager
    {
        public static IEnumerable<Factions.RebelFaction> GetRebelFaction(TaleWorlds.CampaignSystem.Kingdom kingdom)
            => kingdom.Factions;
    }
}
