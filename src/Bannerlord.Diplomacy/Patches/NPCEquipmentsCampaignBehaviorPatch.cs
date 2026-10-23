using Diplomacy.PatchTools;

using HarmonyLib;

using Helpers;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.Core;

namespace Diplomacy.Patches
{
    internal sealed class NPCEquipmentsCampaignBehaviorPatch : PatchClass<NPCEquipmentsCampaignBehaviorPatch, NPCEquipmentsCampaignBehavior>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            // Ruler equipment changes were introduced in v1.4.3. Older supported versions
            // have the behavior, but not this event handler.
            if (AccessTools.DeclaredMethod(typeof(NPCEquipmentsCampaignBehavior), "OnRulingClanChanged") is not null)
                yield return new Transpiler(nameof(OnRulingClanChangedTranspiler), "OnRulingClanChanged");
        }

        private static IEnumerable<CodeInstruction> OnRulingClanChangedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var assignEquipment = AccessTools.DeclaredMethod(typeof(EquipmentHelper), nameof(EquipmentHelper.AssignHeroEquipmentFromEquipment));
            var assignIfAvailable = AccessTools.DeclaredMethod(typeof(NPCEquipmentsCampaignBehaviorPatch), nameof(AssignEquipmentIfAvailable));
            var replacements = 0;

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(assignEquipment))
                {
                    // Preserve labels and exception blocks, and leave all other event logic intact.
                    instruction.operand = assignIfAvailable;
                    replacements++;
                }

                yield return instruction;
            }

            if (replacements == 0)
                throw new InvalidOperationException("Could not find ruler equipment assignments in NPCEquipmentsCampaignBehavior.OnRulingClanChanged.");
        }

        private static void AssignEquipmentIfAvailable(Hero hero, Equipment? equipment)
        {
            // Custom cultures may lack a ruler/lord template for this gender or equipment type.
            // Keep that outfit rather than aborting kingdom creation halfway through its events.
            if (equipment is null)
            {
                LogFactory.Get<NPCEquipmentsCampaignBehaviorPatch>().LogWarning(
                    "No ruler-change equipment available for hero {HeroId} ({HeroName}), culture {CultureId}; keeping current equipment.",
                    hero.StringId, hero.Name, hero.Culture?.StringId);
                return;
            }

            EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment);
        }
    }
}
