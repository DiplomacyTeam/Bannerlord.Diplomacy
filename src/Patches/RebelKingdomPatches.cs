using Diplomacy.Extensions;
using Diplomacy.PatchTools;
using SandBox;
using SandBox.View.Map;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.Patches
{
    internal sealed class RebelKingdomPatches : PatchClass<RebelKingdomPatches>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            Type conversationBehaviorType = Type.GetType("SandBox.LordConversationsCampaignBehavior, SandBox, Version=1.0.0.0, Culture=neutral");
            return new Patch[]
                    {
            new Postfix(nameof(EnforceWarConditions), conversationBehaviorType, "conversation_player_threats_lord_verify_on_condition"),
            new Postfix(nameof(EnforceWarConditions), conversationBehaviorType, "conversation_player_wants_to_make_peace_on_condition"),
            new Postfix(nameof(EnforceWarConditions), conversationBehaviorType, "conversation_lord_request_mission_ask_on_condition"),
            new Postfix(nameof(EnforceWarConditions), conversationBehaviorType, "conversation_player_want_to_join_faction_as_mercenary_or_vassal_on_condition"),
            new Postfix(nameof(DisableKingdomScreen), typeof(MapNavigationHandler), "get_KingdomPermission")
                    };
        }

        private static void DisableKingdomScreen(ref NavigationPermissionItem __result)
        {
            if (__result.IsAuthorized && Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom.IsRebelKingdom())
            {
                __result = new NavigationPermissionItem(false, new TextObject("{=f66sNaiz}You cannot be part of a rebellion"));
            }
        }

        private static void EnforceWarConditions(ref bool __result)
        {
            MobileParty encounteredMobileParty = PlayerEncounter.EncounteredMobileParty;
            Kingdom? encounteredKingdom = encounteredMobileParty?.MapFaction as Kingdom;
            Kingdom? playerKingdom = Clan.PlayerClan.Kingdom;
            if ((encounteredKingdom?.IsRebelKingdom() ?? false) || (playerKingdom?.IsRebelKingdom() ?? false))
            {
                __result = false;
            }
        }
    }
}
