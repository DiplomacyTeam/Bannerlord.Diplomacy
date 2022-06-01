using TaleWorlds.CampaignSystem;

namespace Diplomacy.DiplomaticAction.WarPeace
{
    internal sealed class WarDeclaredEvent
    {
        public IFaction Faction { get; }

        public IFaction ProvocatorFaction { get; }

        public bool IsProvoked { get; }

        public WarDeclaredEvent(IFaction faction, IFaction provocatorFaction, bool isProvoked)
        {
            Faction = faction;
            ProvocatorFaction = provocatorFaction;
            IsProvoked = isProvoked;
        }
    }
}