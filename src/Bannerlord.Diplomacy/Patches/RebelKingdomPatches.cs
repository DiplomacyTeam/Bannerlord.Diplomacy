using Diplomacy.CivilWar.Factions;
using Diplomacy.Extensions;
using Diplomacy.PatchTools;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;

namespace Diplomacy.Patches
{
    internal sealed class RebelKingdomPatches : PatchClass<RebelKingdomPatches>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            var conversationBehaviorType = Type.GetType("TaleWorlds.CampaignSystem.CampaignBehaviors.LordConversationsCampaignBehavior, TaleWorlds.CampaignSystem, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")!;

            return new Patch[]
            {
            new Postfix(nameof(PreventOtherActionsConversation), conversationBehaviorType, "conversation_lord_request_mission_ask_on_condition"),
            new Postfix(nameof(PreventDiplomaticActionsConversation), conversationBehaviorType, "conversation_player_wants_to_make_peace_on_condition"),
            new Postfix(nameof(PreventDiplomaticActionsConversation), conversationBehaviorType, "conversation_player_want_to_join_faction_as_mercenary_or_vassal_on_condition"),
            new Postfix(nameof(PreventHostileActionsConversation), conversationBehaviorType, "conversation_player_threats_lord_verify_on_condition"),
            new Postfix(nameof(PreventHostileActionsConversation), typeof(VillagerCampaignBehavior), "village_farmer_loot_on_condition"),
            new Postfix(nameof(PreventHostileActionsConversation), typeof(CaravansCampaignBehavior), "caravan_loot_on_condition"),
            new Postfix(nameof(PreventHostileActionsMenu), typeof(VillageHostileActionCampaignBehavior), "game_menu_village_hostile_action_on_condition"),
            new Prefix(nameof(HandleThroneAbdication), typeof(KingdomManager), "AbdicateTheThrone"),
            };
        }

        private static void PreventHostileActionsConversation(ref bool __result)
        {
            if (!__result)
            {
                return;
            }

            var conversationFaction = GetConversationFaction();
            if (conversationFaction is null)
            {
                return;
            }

            __result = !ShouldPreventHostileAction(conversationFaction);
        }

        private static void PreventHostileActionsMenu(ref bool __result)
        {
            if (!__result)
            {
                return;
            }

            var village = Settlement.CurrentSettlement?.Village;
            if (village?.Owner is null)
            {
                return;
            }

            __result = !ShouldPreventHostileAction(village.Owner.MapFaction);
        }

        private static void PreventDiplomaticActionsConversation(ref bool __result)
        {
            if (!__result)
            {
                return;
            }

            var conversationFaction = GetConversationFaction();
            if (conversationFaction is null)
            {
                return;
            }

            var shouldPreventAction = conversationFaction.MapFaction is Kingdom encounteredKingdom && encounteredKingdom.IsRebelKingdom();
            __result = !shouldPreventAction;
        }

        private static void PreventOtherActionsConversation(ref bool __result)
        {
            if (!__result)
            {
                return;
            }

            var conversationFaction = GetConversationFaction();
            if (conversationFaction is null)
            {
                return;
            }

            var shouldPreventAction = conversationFaction.MapFaction is Kingdom encounteredKingdom && encounteredKingdom.IsRebelKingdom() &&
                                 Clan.PlayerClan.MapFaction != conversationFaction.MapFaction;
            __result = !shouldPreventAction;
        }

        // Neither the conversation party nor the one-to-one hero is guaranteed to be set while these
        // conditions are evaluated, so both have to be treated as optional.
        private static IFaction? GetConversationFaction()
        {
            var conversationManager = Campaign.Current?.ConversationManager;
            return conversationManager?.ConversationParty?.MapFaction ?? conversationManager?.OneToOneConversationHero?.MapFaction;
        }

        private static bool ShouldPreventHostileAction(IFaction otherFaction)
        {
            var playerFaction = Clan.PlayerClan.MapFaction;
            var shouldPreventAction1 = otherFaction is Kingdom encounteredKingdom && encounteredKingdom.IsRebelKingdom() && !encounteredKingdom.IsAtWarWith(playerFaction);
            var shouldPreventAction2 =
                playerFaction is Kingdom playerKingdom && playerKingdom.IsRebelKingdom() && !playerKingdom.IsAtWarWith(otherFaction);
            return shouldPreventAction1 || shouldPreventAction2;
        }

        private static bool HandleThroneAbdication(Kingdom kingdom)
        {
            RebelFaction? enforcedFaction = null;

            if (kingdom.Clans.Count <= 1 && kingdom.HasRebellion())
            {
                enforcedFaction = kingdom.GetRebelFactions().First();
                enforcedFaction.EnforceSuccess();
            }

            if (kingdom.Clans.Count > 1 && kingdom.HasRebellion() && kingdom.GetRebelFactions().FirstOrDefault() is AbdicationFaction abdicationFaction)
            {
                enforcedFaction = abdicationFaction;
                abdicationFaction.EnforceSuccess();
            }

            // An abdication faction's success already puts the throne to a vote. The game's abdication would then
            // hand the throne to another clan and hold a second, enforced vote.
            return enforcedFaction is not AbdicationFaction;
        }
    }
}