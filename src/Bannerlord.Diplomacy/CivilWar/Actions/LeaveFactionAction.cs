using Diplomacy.CivilWar.Factions;

using TaleWorlds.CampaignSystem;

namespace Diplomacy.CivilWar.Actions
{
    public class LeaveFactionAction
    {
        public static void Apply(Clan clan, RebelFaction rebelFaction)
        {
            rebelFaction.RemoveClan(clan);
        }
    }
}