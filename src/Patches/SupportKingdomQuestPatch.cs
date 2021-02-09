using Diplomacy.PatchTools;

using System;
using System.Collections.Generic;
using System.Reflection;

using StoryMode.Behaviors.Quests;
using TaleWorlds.CampaignSystem;

namespace Diplomacy.Patches
{
    internal sealed class SupportKingdomQuestPatch : PatchClass<SupportKingdomQuestPatch>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Postfix(nameof(MainStoryLineChosenPostfix), _TargetType, "MainStoryLineChosen"),
        };

        private static readonly Type _TargetType = typeof(SupportKingdomQuestBehavior)
            .GetNestedType("SupportKingdomQuest", BindingFlags.NonPublic | BindingFlags.Instance);

        private delegate void CompleteQuestWithSuccessDel(QuestBase instance);

        private static readonly CompleteQuestWithSuccessDel _CompleteQuestWithSuccess = new Reflect.Method<QuestBase>("CompleteQuestWithSuccess")
            .GetOpenDelegate<CompleteQuestWithSuccessDel>();

        private static void MainStoryLineChosenPostfix(object __instance)
        {
            var quest = (QuestBase)__instance;

            if (!quest.IsFinalized)
                _CompleteQuestWithSuccess(quest);
        }
    }
}
