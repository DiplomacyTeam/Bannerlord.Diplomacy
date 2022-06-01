using Diplomacy.PatchTools;

using StoryMode.Quests.FirstPhase;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;

namespace Diplomacy.Patches
{
    internal sealed class SupportKingdomQuestPatch : PatchClass<SupportKingdomQuestPatch>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Postfix(nameof(MainStoryLineChosenPostfix), TargetType, "MainStoryLineChosen"),
        };

        private static readonly Type TargetType = typeof(SupportKingdomQuest);

        private static readonly Action<QuestBase> CompleteQuestWithSuccess = new Reflect.Method<QuestBase>("CompleteQuestWithSuccess")
            .GetOpenDelegate<Action<QuestBase>>();

        private static void MainStoryLineChosenPostfix(object __instance)
        {
            var quest = (QuestBase) __instance;

            if (!quest.IsFinalized)
                CompleteQuestWithSuccess(quest);
        }
    }
}