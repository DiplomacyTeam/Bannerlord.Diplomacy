namespace Diplomacy.CivilWar.Factions
{
    public enum RebelDemandType { Abdication, Secession }

    public sealed class RebelFaction
    {
        public TaleWorlds.CampaignSystem.Clan SponsorClan { get; set; } = null!;
        public TaleWorlds.CampaignSystem.Kingdom ParentKingdom { get; set; } = null!;
        public RebelDemandType RebelDemandType { get; set; }
        public bool AtWar { get; set; }
        public List<TaleWorlds.CampaignSystem.Clan> Clans { get; } = new();
        public void AddClan(TaleWorlds.CampaignSystem.Clan clan) => Clans.Add(clan);
    }
}
