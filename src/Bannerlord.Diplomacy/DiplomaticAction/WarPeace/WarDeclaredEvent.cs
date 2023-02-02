#if v100 || v101 || v102 || v103
using TaleWorlds.CampaignSystem;

namespace Diplomacy.DiplomaticAction.WarPeace
{
    public readonly struct WarDeclaredEvent
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
#endif