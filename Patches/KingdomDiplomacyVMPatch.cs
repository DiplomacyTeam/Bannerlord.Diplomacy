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
using TaleWorlds.Library;
using DiplomacyFixes.ViewModel;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace DiplomacyFixes.Patches
{
    [HarmonyPatch(typeof(KingdomDiplomacyVM))]
    class KingdomDiplomacyVMPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnDeclareWar")]
        public static bool OnDeclareWarPatch(KingdomTruceItemVM item, KingdomDiplomacyVM __instance)
        {
            List<TextObject> warExceptions = WarAndPeaceConditions.CanDeclareWarExceptions(item);
            if (warExceptions.IsEmpty())
            {
                float influenceCost = DiplomacyCostCalculator.DetermineInfluenceCostForDeclaringWar();
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
            }
            else
            {
                MessageHelper.SendFailedActionMessage(new TextObject("{=v0cDMIcl}Cannot declare war on this kingdom. ").ToString(), warExceptions);
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnDeclarePeace")]
        public static bool OnDeclarePeacePatch(KingdomWarItemVM item, KingdomDiplomacyVM __instance)
        {
            List<TextObject> peaceExceptions = WarAndPeaceConditions.CanMakePeaceExceptions(item);
            if (peaceExceptions.IsEmpty())
            {
                float influenceCost = DiplomacyCostCalculator.DetermineInfluenceCostForMakingPeace();
                DiplomacyCostManager.deductInfluenceFromPlayerClan(influenceCost);
                MakePeaceAction.Apply(item.Faction1, item.Faction2);
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
                MessageHelper.SendFailedActionMessage(new TextObject("{=Pqk3WuGz}Cannot make peace with this kingdom. ").ToString(), peaceExceptions);
            }
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("RefreshDiplomacyList")]
        public static void RefreshDiplomacyListPatch(KingdomDiplomacyVM __instance)
        {
            MBBindingList<KingdomWarItemVM> playerWars = new MBBindingList<KingdomWarItemVM>();
            MBBindingList<KingdomTruceItemVM> playerTruces = new MBBindingList<KingdomTruceItemVM>();

            MethodInfo onDiplomacyItemSelection = __instance.GetType().GetMethod("OnDiplomacyItemSelection", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo onDeclareWarMethod = __instance.GetType().GetMethod("OnDeclareWar", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo onProposePeaceMethod = __instance.GetType().GetMethod("OnDeclarePeace", BindingFlags.NonPublic | BindingFlags.Instance);

            Action<KingdomTruceItemVM> onDeclareWarAction = (Action<KingdomTruceItemVM>)Delegate.CreateDelegate(typeof(Action<KingdomTruceItemVM>), __instance, onDeclareWarMethod);
            Action<KingdomWarItemVM> onProposePeaceAction = (Action<KingdomWarItemVM>)Delegate.CreateDelegate(typeof(Action<KingdomWarItemVM>), __instance, onProposePeaceMethod);
            Action<KingdomDiplomacyItemVM> onItemSelectedAction = (Action<KingdomDiplomacyItemVM>)Delegate.CreateDelegate(typeof(Action<KingdomDiplomacyItemVM>), __instance, onDiplomacyItemSelection);

            Kingdom playerKingdom = Clan.PlayerClan.Kingdom;

            foreach (CampaignWar campaignWar in from w in FactionManager.Instance.CampaignWars
                                                orderby w.Side1[0].Name.ToString()
                                                select w)
            {
                if (!campaignWar.Side1[0].IsMinorFaction && !campaignWar.Side2[0].IsMinorFaction && (campaignWar.Side1[0] == playerKingdom || campaignWar.Side2[0] == playerKingdom))
                {
                    playerWars.Add(new KingdomWarItemVMExtensionVM(campaignWar, onItemSelectedAction, onProposePeaceAction));
                }
            }
            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (kingdom != playerKingdom && !kingdom.IsDeactivated && (FactionManager.IsAlliedWithFaction(kingdom, playerKingdom) || FactionManager.IsNeutralWithFaction(kingdom, playerKingdom)))
                {
                    playerTruces.Add(new KingdomTruceItemVMExtensionVM(playerKingdom, kingdom, onItemSelectedAction, onDeclareWarAction));
                }
            }

            __instance.PlayerTruces = playerTruces;
            __instance.PlayerWars = playerWars;

            MethodInfo setDefaultSelectedItem = __instance.GetType().GetMethod("SetDefaultSelectedItem", BindingFlags.NonPublic | BindingFlags.Instance);
            setDefaultSelectedItem.Invoke(__instance, new object[] { });
        }
    }
}
