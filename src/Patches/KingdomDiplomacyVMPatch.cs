using Diplomacy.ViewModel;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Library;

namespace Diplomacy.Patches
{
    [HarmonyPatch(typeof(KingdomDiplomacyVM))]
    class KingdomDiplomacyVMPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("RefreshDiplomacyList")]
        public static void RefreshDiplomacyListPatch(KingdomDiplomacyVM __instance)
        {
            var playerWars = new MBBindingList<KingdomWarItemVM>();
            var playerTruces = new MBBindingList<KingdomTruceItemVM>();

            var onDiplomacyItemSelection = typeof(KingdomDiplomacyVM).GetMethod("OnDiplomacyItemSelection", BindingFlags.NonPublic | BindingFlags.Instance);
            var onDeclareWarMethod = typeof(KingdomDiplomacyVM).GetMethod("OnDeclareWar", BindingFlags.NonPublic | BindingFlags.Instance);
            var onProposePeaceMethod = typeof(KingdomDiplomacyVM).GetMethod("OnDeclarePeace", BindingFlags.NonPublic | BindingFlags.Instance);

            var onDeclareWarAction = (Action<KingdomTruceItemVM>)Delegate.CreateDelegate(typeof(Action<KingdomTruceItemVM>), __instance, onDeclareWarMethod);
            var onProposePeaceAction = (Action<KingdomWarItemVM>)Delegate.CreateDelegate(typeof(Action<KingdomWarItemVM>), __instance, onProposePeaceMethod);
            var onItemSelectedAction = (Action<KingdomDiplomacyItemVM>)Delegate.CreateDelegate(typeof(Action<KingdomDiplomacyItemVM>), __instance, onDiplomacyItemSelection);

            var playerKingdom = Clan.PlayerClan.Kingdom;

            foreach (var stanceLink in from x in playerKingdom.Stances
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
            foreach (var kingdom in Kingdom.All)
            {
                if (kingdom != playerKingdom && !kingdom.IsEliminated && FactionManager.IsNeutralWithFaction(kingdom, playerKingdom))
                {
                    playerTruces.Add(new KingdomTruceItemVMExtensionVM(playerKingdom, kingdom, onItemSelectedAction, onDeclareWarAction));
                }
            }

            __instance.PlayerTruces = playerTruces;
            __instance.PlayerWars = playerWars;

            var setDefaultSelectedItem = typeof(KingdomDiplomacyVM).GetMethod("SetDefaultSelectedItem", BindingFlags.NonPublic | BindingFlags.Instance);
            setDefaultSelectedItem.Invoke(__instance, new object[] { });
        }
    }
}
