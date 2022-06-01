using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;

using static HarmonyLib.AccessTools;

namespace Diplomacy
{
    public static class FieldAccessHelper
    {
        public static readonly FieldRef<PlayerEncounter, PartyBase> PlayerEncounterAttackerPartyByRef = FieldRefAccess<PlayerEncounter, PartyBase>("_attackerParty");
        public static readonly FieldRef<PlayerEncounter, PartyBase> PlayerEncounterDefenderPartyByRef = FieldRefAccess<PlayerEncounter, PartyBase>("_defenderParty");
    }
}