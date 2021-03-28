using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;

namespace Diplomacy.Character
{
    
    internal class PlayerCharacterTraitHelper
    {
        private static Action<TraitObject, int, ActionNotes, Hero> AddPlayerTraitXPAndLogEntry = AccessTools.MethodDelegate<Action<TraitObject, int, ActionNotes, Hero>>(AccessTools.Method(typeof(TraitLevelingHelper), "AddPlayerTraitXPAndLogEntry"));

        public static void UpdateTrait(TraitObject trait, int xpValue, ActionNotes context = ActionNotes.DefaultNote, Hero? referenceHero = null)
        {
            AddPlayerTraitXPAndLogEntry(trait, xpValue, context, referenceHero!);
        }

        public static void UpdateTrait(PlayerCharacterTraitEventExperience eventExperience)
        {
            UpdateTrait(eventExperience.Trait, eventExperience.Experience);
        }
    }
}
