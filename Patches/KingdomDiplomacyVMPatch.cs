using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using HarmonyLib;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DiplomacyFixes.Patches
{
    [HarmonyPatch(typeof(KingdomDiplomacyVM))]
    class KingdomDiplomacyVMPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnDeclareWar")]
        public static bool OnDeclareWarPatch(KingdomTruceItemVM item, KingdomDiplomacyVM __instance)
        {
            List<string> warExceptions = WarAndPeaceConditions.canDeclareWarExceptions(item);
            if (warExceptions.IsEmpty())
            {
                float influenceCost = CostCalculator.determineInfluenceCostForDeclaringWar();
                CostUtil.deductInfluenceFromPlayerClan(influenceCost);
                DeclareWarAction.Apply(item.Faction1, item.Faction2);
                try
                {
                    __instance.RefreshValues();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "");
                }
            }
            else
            {
                MessageHelper.SendFailedActionMessage("Cannot declare war on this kingdom. ", warExceptions);
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnDeclarePeace")]
        public static bool OnDeclarePeacePatch(KingdomWarItemVM item, KingdomDiplomacyVM __instance)
        {
            List<string> peaceExceptions = WarAndPeaceConditions.canMakePeaceExceptions(item);
            if (peaceExceptions.IsEmpty())
            {
                float influenceCost = CostCalculator.determineInfluenceCostForMakingPeace();
                CostUtil.deductInfluenceFromPlayerClan(influenceCost);
                MakePeaceAction.Apply(item.Faction1, item.Faction2);
                try
                {
                    __instance.RefreshValues();
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message, "");
                }
            }
            else
            {
                MessageHelper.SendFailedActionMessage("Cannot make peace with this kingdom. ", peaceExceptions);
            }
            return false;
        }
    }
}
