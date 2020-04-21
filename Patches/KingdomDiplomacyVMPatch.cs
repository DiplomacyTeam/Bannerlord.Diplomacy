using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using HarmonyLib;
using TaleWorlds.CampaignSystem.Actions;

namespace DiplomacyFixes.Patches
{
    [HarmonyPatch(typeof(KingdomDiplomacyVM))]
    class KingdomDiplomacyVMPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnDeclareWar")]
        public static bool OnDeclareWarPatch(KingdomTruceItemVM item, KingdomDiplomacyVM __instance)
        {
            float influenceCost = CostCalculator.determineInfluenceCostForDeclaringWar();
            CostUtil.deductInfluenceFromPlayerClan(influenceCost);
            DeclareWarAction.Apply(item.Faction1, item.Faction2);
            __instance.RefreshValues();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnDeclarePeace")]
        public static bool OnDeclarePeacePatch(KingdomWarItemVM item, KingdomDiplomacyVM __instance)
        {
            float influenceCost = CostCalculator.determineInfluenceCostForMakingPeace();
            CostUtil.deductInfluenceFromPlayerClan(influenceCost);
            MakePeaceAction.Apply(item.Faction1, item.Faction2);
            __instance.RefreshValues();
            return false;
        }
    }
}
