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

            foreach (StanceLink stanceLink in from x in playerKingdom.Stances
                                              where x.IsAtWar
                                              select x into w
                                              orderby w.Faction1.Name.ToString() + w.Faction2.Name.ToString()
                                              select w)
            {
                if (stanceLink.Faction1 is Kingdom && stanceLink.Faction2 is Kingdom && !stanceLink.Faction1.IsMinorFaction && !stanceLink.Faction2.IsMinorFaction)
                {
                    playerWars.Add(new KingdomWarItemVMExtensionVM(stanceLink, onItemSelectedAction, onProposePeaceAction));
                }
            }
            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (kingdom != playerKingdom && !kingdom.IsEliminated && FactionManager.IsNeutralWithFaction(kingdom, playerKingdom))
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
