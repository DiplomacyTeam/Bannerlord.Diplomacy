using JetBrains.Annotations;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace Diplomacy.Patches
{
    [HarmonyPatch(typeof(UrbanCharactersCampaignBehavior))]
    [UsedImplicitly]
    internal sealed class UrbanCharactersCampaignBehaviorPatch
    {
        private static readonly FieldInfo CompanionsFieldInfo = AccessTools.Field(typeof(UrbanCharactersCampaignBehavior), "_companions");
        private static readonly Action<Hero, UrbanCharactersCampaignBehavior> ActivateCharacter = (hero, __instance) =>
        {
            hero.ChangeState(Hero.CharacterStates.Active);
            var companionsList = (List<Hero>)CompanionsFieldInfo.GetValue(__instance);
            companionsList.Remove(hero);
        };


        [HarmonyPostfix, HarmonyPatch("RegisterEvents")]
        [UsedImplicitly]
        public static void RegisterEventsPatch(UrbanCharactersCampaignBehavior __instance)
        {
            CampaignEvents.NewCompanionAdded.AddNonSerializedListener(__instance, hero => ActivateCharacter(hero, __instance));
        }

//      It Seems they changed this to "DailyTickHero" in 1.5.0
        //[HarmonyPostfix, HarmonyPatch("DailyTick")]
        [HarmonyPostfix, HarmonyPatch("DailyTickHero")]
        [UsedImplicitly]
        public static void HourlyTickPatch(UrbanCharactersCampaignBehavior __instance)
        {
            Hero.MainHero.CompanionsInParty.Where(companion => companion.HeroState == Hero.CharacterStates.NotSpawned).Do(hero => ActivateCharacter(hero, __instance));
        }
    }
}
