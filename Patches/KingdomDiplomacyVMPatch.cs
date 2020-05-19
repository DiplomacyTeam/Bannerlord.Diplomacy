using DiplomacyFixes.ViewModel;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Library;

namespace DiplomacyFixes.Patches
{
    [HarmonyPatch(typeof(KingdomDiplomacyVM))]
    class KingdomDiplomacyVMPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnDeclareWar")]
        public static bool OnDeclareWarPatch(KingdomTruceItemVM item, KingdomDiplomacyVM __instance)
        {
            float influenceCost = DiplomacyCostCalculator.DetermineInfluenceCostForDeclaringWar(item.Faction1 as Kingdom);
            DiplomacyCostManager.deductInfluenceFromPlayerClan(influenceCost);
            DeclareWarAction.Apply(item.Faction1, item.Faction2);
            try
            {
                __instance.RefreshValues();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "");
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnDeclarePeace")]
        public static bool OnDeclarePeacePatch(KingdomWarItemVM item, KingdomDiplomacyVM __instance)
        {
            KingdomPeaceAction.ApplyPeace(item.Faction1 as Kingdom, item.Faction2 as Kingdom, forcePlayerCharacterCosts: true);
            try
            {
                __instance.RefreshValues();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "");
            }
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("RefreshDiplomacyList")]
        public static void RefreshDiplomacyListPatch(KingdomDiplomacyVM __instance)
        {
            MBBindingList<KingdomWarItemVM> playerWars = new MBBindingList<KingdomWarItemVM>();
            MBBindingList<KingdomTruceItemVM> playerTruces = new MBBindingList<KingdomTruceItemVM>();

            MethodInfo onDiplomacyItemSelection = typeof(KingdomDiplomacyVM).GetMethod("OnDiplomacyItemSelection", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo onDeclareWarMethod = typeof(KingdomDiplomacyVM).GetMethod("OnDeclareWar", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo onProposePeaceMethod = typeof(KingdomDiplomacyVM).GetMethod("OnDeclarePeace", BindingFlags.NonPublic | BindingFlags.Instance);

            Action<KingdomTruceItemVM> onDeclareWarAction = (Action<KingdomTruceItemVM>)Delegate.CreateDelegate(typeof(Action<KingdomTruceItemVM>), __instance, onDeclareWarMethod);
            Action<KingdomWarItemVM> onProposePeaceAction = (Action<KingdomWarItemVM>)Delegate.CreateDelegate(typeof(Action<KingdomWarItemVM>), __instance, onProposePeaceMethod);
            Action<KingdomDiplomacyItemVM> onItemSelectedAction = (Action<KingdomDiplomacyItemVM>)Delegate.CreateDelegate(typeof(Action<KingdomDiplomacyItemVM>), __instance, onDiplomacyItemSelection);

            Kingdom playerKingdom = Clan.PlayerClan.Kingdom;

            foreach (CampaignWar campaignWar in from w in FactionManager.Instance.CampaignWars
                                                orderby w.Side1[0].Name.ToString()
                                                select w)
            {
                if (campaignWar.Side1[0] is Kingdom && campaignWar.Side2[0] is Kingdom && !campaignWar.Side1[0].IsMinorFaction && !campaignWar.Side2[0].IsMinorFaction && (campaignWar.Side1[0] == playerKingdom || campaignWar.Side2[0] == playerKingdom))
                {
                    playerWars.Add(new KingdomWarItemVMExtensionVM(campaignWar, onItemSelectedAction, onProposePeaceAction));
                }
            }
            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (kingdom != playerKingdom && !kingdom.IsDeactivated && FactionManager.IsNeutralWithFaction(kingdom, playerKingdom))
                {
                    playerTruces.Add(new KingdomTruceItemVMExtensionVM(playerKingdom, kingdom, onItemSelectedAction, onDeclareWarAction));
                }
            }

            __instance.PlayerTruces = playerTruces;
            __instance.PlayerWars = playerWars;

            MethodInfo setDefaultSelectedItem = typeof(KingdomDiplomacyVM).GetMethod("SetDefaultSelectedItem", BindingFlags.NonPublic | BindingFlags.Instance);
            setDefaultSelectedItem.Invoke(__instance, new object[] { });
        }
    }
}
