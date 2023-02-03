using HarmonyLib.BUTR.Extensions;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;

namespace Diplomacy.Character
{
    internal class PlayerCharacterTraitHelper
    {
        private delegate void AddPlayerTraitXPAndLogEntryDelegate(TraitObject trait, int xpValue, ActionNotes context, Hero referenceHero);
        private static readonly AddPlayerTraitXPAndLogEntryDelegate? AddPlayerTraitXPAndLogEntry =
            AccessTools2.GetDelegate<AddPlayerTraitXPAndLogEntryDelegate>(typeof(TraitLevelingHelper), "AddPlayerTraitXPAndLogEntry");

        public static void UpdateTrait(TraitObject trait, int xpValue, ActionNotes context = ActionNotes.DefaultNote, Hero? referenceHero = null)
        {
            AddPlayerTraitXPAndLogEntry?.Invoke(trait, xpValue, context, referenceHero!);
        }

        public static void UpdateTrait(PlayerCharacterTraitEventExperience eventExperience)
        {
            UpdateTrait(eventExperience.Trait, eventExperience.Experience);
        }
    }
}