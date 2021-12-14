﻿using Diplomacy.PatchTools;

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
            new Postfix(nameof(MainStoryLineChosenPostfix), TargetType, "MainStoryLineChosen"),
        };

#if e170
        private static readonly Type TargetType = typeof(SupportKingdomQuest);
#else
        private static readonly Type TargetType = typeof(SupportKingdomQuestBehavior)
            .GetNestedType("SupportKingdomQuest", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
#endif

        private static readonly Action<QuestBase> CompleteQuestWithSuccess = new Reflect.Method<QuestBase>("CompleteQuestWithSuccess")
            .GetOpenDelegate<Action<QuestBase>>();

        private static void MainStoryLineChosenPostfix(object __instance)
        {
            var quest = (QuestBase)__instance;

            if (!quest.IsFinalized)
                CompleteQuestWithSuccess(quest);
        }
    }
}