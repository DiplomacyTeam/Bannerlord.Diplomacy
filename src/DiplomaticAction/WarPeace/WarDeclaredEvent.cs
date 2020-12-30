using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.DiplomaticAction.WarPeace
{
    struct WarDeclaredEvent
    {
        public IFaction Faction { get; }
        public IFaction ProvocatorFaction { get; }
        public bool IsProvoked { get; }

        public WarDeclaredEvent(IFaction faction, IFaction provocatorFaction, bool isProvoked)
        {
            this.Faction = faction;
            this.ProvocatorFaction = provocatorFaction;
            this.IsProvoked = isProvoked;
        }
    }
}
