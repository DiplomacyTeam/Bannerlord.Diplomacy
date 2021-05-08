using Diplomacy.Extensions;
using Diplomacy.PatchTools;
using Diplomacy.ViewModel;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Diplomacy.Patches
{
    internal sealed class KingdomDiplomacyVMPatch : PatchClass<KingdomDiplomacyVMPatch, KingdomDiplomacyVM>
    {
        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Postfix(nameof(RefreshDiplomacyListPostfix), nameof(KingdomDiplomacyVM.RefreshDiplomacyList)),
        };

        private static readonly Reflect.Method<KingdomDiplomacyVM> _OnDiplomacyItemSelectionRM = new("OnDiplomacyItemSelection");
        private static readonly Reflect.Method<KingdomDiplomacyVM> _OnDeclareWarRM = new("OnDeclareWar");
        private static readonly Reflect.Method<KingdomDiplomacyVM> _OnDeclarePeaceRM = new("OnDeclarePeace");

        private delegate void SetDefaultSelectedItemDel(KingdomDiplomacyVM instance);

        private static readonly SetDefaultSelectedItemDel _SetDefaultSelectedItem = new Reflect.Method<KingdomDiplomacyVM>("SetDefaultSelectedItem")
            .GetOpenDelegate<SetDefaultSelectedItemDel>();

        private static void RefreshDiplomacyListPostfix(KingdomDiplomacyVM __instance)
        {
            var playerWars = new MBBindingList<KingdomWarItemVM>();
            var playerTruces = new MBBindingList<KingdomTruceItemVM>();
            var playerKingdom = Clan.PlayerClan.Kingdom;

            var onDiplomacyItemSelection = _OnDiplomacyItemSelectionRM.GetDelegate<Action<KingdomDiplomacyItemVM>>(__instance);
            var onDeclareWar = _OnDeclareWarRM.GetDelegate<Action<KingdomTruceItemVM>>(__instance);
            var onDeclarePeace = _OnDeclarePeaceRM.GetDelegate<Action<KingdomWarItemVM>>(__instance);

            foreach (var stanceLink in from x in playerKingdom.Stances
                                              where x.IsAtWar
                                              select x into w
                                              orderby w.Faction1.Name.ToString() + w.Faction2.Name.ToString()
                                              select w)
            {
                if (stanceLink.Faction1.IsKingdomFaction && stanceLink.Faction2.IsKingdomFaction && !(stanceLink.Faction1 as Kingdom)!.IsRebelKingdom() && !(stanceLink.Faction2 as Kingdom)!.IsRebelKingdom())
                {
                    // FIXME: LO-PRIO: Verify the implicit downcast for onDiplomacyItemSelection from an Action<KingdomDiplomacyItemVM> to the
                    // parameter target type Action<KingdomWarItemVM>. KingdomWarItemVM inherits from KingdomDiplomacyItemVM, not the opposite.
                    playerWars.Add(new KingdomWarItemVMExtensionVM(stanceLink, onDiplomacyItemSelection, onDeclarePeace));
                }
            }

            foreach (var kingdom in Kingdom.All.Where(k => k != playerKingdom
                                                        && !k.IsEliminated
                                                        && FactionManager.IsNeutralWithFaction(k, playerKingdom)
                                                        && !k.IsRebelKingdom()))
            {
                playerTruces.Add(new KingdomTruceItemVMExtensionVM(playerKingdom, kingdom, onDiplomacyItemSelection, onDeclareWar));
            }

            __instance.PlayerTruces = playerTruces;
            __instance.PlayerWars = playerWars;

            GameTexts.SetVariable("STR", __instance.PlayerWars.Count);
            __instance.NumOfPlayerWarsText = GameTexts.FindText("str_STR_in_parentheses", null).ToString();
            GameTexts.SetVariable("STR", __instance.PlayerTruces.Count);
            __instance.NumOfPlayerTrucesText = GameTexts.FindText("str_STR_in_parentheses", null).ToString();

            _SetDefaultSelectedItem(__instance);
        }
    }
}
