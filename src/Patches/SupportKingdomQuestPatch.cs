using HarmonyLib;
using StoryMode.Behaviors.Quests;
using System.Reflection;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.Patches
{
    [HarmonyPatch]
    class SupportKingdomQuestPatch
    {
        [HarmonyTargetMethod]
        static MethodBase TargetMethod()
        {
            return typeof(SupportKingdomQuestBehavior).GetNestedType("SupportKingdomQuest", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("MainStoryLineChosen", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [HarmonyPostfix]
        public static void PostFix(object __instance)
        {
            QuestBase questBase = ((QuestBase)__instance);
            if (!questBase.IsFinalized)
            {
                typeof(QuestBase).GetMethod("CompleteQuestWithSuccess", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(questBase, new object[] { });
            }
        }
    }
}
