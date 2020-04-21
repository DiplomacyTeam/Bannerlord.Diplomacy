using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace DiplomacyFixes.Patches
{
    [HarmonyPatch(typeof(MakePeaceAction))]
    class MakePeaceActionPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Apply")]
        public static void ApplyPatch(IFaction faction1, IFaction faction2)
        {
            if (Hero.MainHero.MapFaction.Equals(faction1) || Hero.MainHero.MapFaction.Equals(faction2))
            {
                IFaction factionToUpdate = Hero.MainHero.MapFaction.Equals(faction1) ? faction2 : faction1;
                CooldownManager.UpdateLastWarTime(factionToUpdate, CampaignTime.Now);
            }
        }
    }
}
