using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.Party;

using static HarmonyLib.AccessTools;

namespace Diplomacy.Helpers
{
    internal static class FieldAccessHelper
    {
        public static readonly FieldRef<PlayerEncounter, PartyBase> PlayerEncounterAttackerPartyByRef = FieldRefAccess<PlayerEncounter, PartyBase>("_attackerParty");
        public static readonly FieldRef<PlayerEncounter, PartyBase> PlayerEncounterDefenderPartyByRef = FieldRefAccess<PlayerEncounter, PartyBase>("_defenderParty");

        public static readonly StructFieldRef<CampaignTime, long> CampaignTimeNumTicksByRef = StructFieldRefAccess<CampaignTime, long>("_numTicks");

        public static readonly FieldRef<CharacterKilledLogEntry, KillCharacterAction.KillCharacterActionDetail> CharacterKilledLogEntryActionDetailByRef = FieldRefAccess<CharacterKilledLogEntry, KillCharacterAction.KillCharacterActionDetail>("_actionDetail");
    }
}