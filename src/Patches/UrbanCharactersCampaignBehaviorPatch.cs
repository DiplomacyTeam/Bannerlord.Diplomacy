using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace DiplomacyFixes.Patches
{
    [HarmonyPatch(typeof(UrbanCharactersCampaignBehavior))]
    class UrbanCharactersCampaignBehaviorPatch
    {
        private static FieldInfo _companionsFieldInfo = typeof(UrbanCharactersCampaignBehavior).GetField("_companions", BindingFlags.Instance | BindingFlags.NonPublic);
        private static Action<Hero, UrbanCharactersCampaignBehavior> ActivateCharacter = (hero, __instance) =>
        {
            hero.ChangeState(Hero.CharacterStates.Active);
            List<Hero> companionsList = (List<Hero>)_companionsFieldInfo.GetValue(__instance);
            companionsList.Remove(hero);
        };


        [HarmonyPostfix, HarmonyPatch("RegisterEvents")]
        public static void RegisterEventsPatch(UrbanCharactersCampaignBehavior __instance)
        {
            CampaignEvents.NewCompanionAdded.AddNonSerializedListener(__instance, hero => ActivateCharacter(hero, __instance));
        }

//      It Seems they changed this to "DailyTickHero" in 1.5.0
        //[HarmonyPostfix, HarmonyPatch("DailyTick")]
        [HarmonyPostfix, HarmonyPatch("DailyTickHero")]
        public static void HourlyTickPatch(UrbanCharactersCampaignBehavior __instance)
        {
            Hero.MainHero.CompanionsInParty.Where(companion => companion.HeroState == Hero.CharacterStates.NotSpawned).Do(hero => ActivateCharacter(hero, __instance));
        }
    }
}
