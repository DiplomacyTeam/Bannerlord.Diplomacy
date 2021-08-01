using Diplomacy.Extensions;
using Diplomacy.PatchTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Diplomacy.CivilWar.Factions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.Towns;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.VillageBehaviors;

namespace Diplomacy.Patches
{
    internal sealed class RebelKingdomPatches : PatchClass<RebelKingdomPatches>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            Type conversationBehaviorType = Type.GetType("SandBox.LordConversationsCampaignBehavior, SandBox, Version=1.0.0.0, Culture=neutral")!;
            return new Patch[]
                    {
            new Postfix(nameof(EnforceWarConditionsConversation), conversationBehaviorType, "conversation_player_threats_lord_verify_on_condition"),
            new Postfix(nameof(EnforceWarConditionsConversation), conversationBehaviorType, "conversation_player_wants_to_make_peace_on_condition"),
            new Postfix(nameof(EnforceWarConditionsConversation), conversationBehaviorType, "conversation_lord_request_mission_ask_on_condition"),
            new Postfix(nameof(EnforceWarConditionsConversation), conversationBehaviorType, "conversation_player_want_to_join_faction_as_mercenary_or_vassal_on_condition"),
            new Postfix(nameof(EnforceWarConditionsConversation), typeof(VillagerCampaignBehavior), "village_farmer_loot_on_condition"),
            new Postfix(nameof(EnforceWarConditionsConversation), typeof(CaravansCampaignBehavior), "caravan_loot_on_condition"),
            new Postfix(nameof(EnforceWarConditionsMenu), typeof(PlayerTownVisitCampaignBehavior), "game_menu_village_hostile_action_on_condition"),
            new Prefix(nameof(HandleThroneAbdication), typeof(KingdomManager), "AbdicateTheThrone"),
                    };
        }

        private static void EnforceWarConditionsConversation(ref bool __result)
        {
            MobileParty conversationParty = Campaign.Current.ConversationManager.ConversationParty;
            __result = !ShouldPreventAction(conversationParty.MapFaction);
        }

        private static void EnforceWarConditionsMenu(ref bool __result)
        {
            Village village = Settlement.CurrentSettlement.Village;
            __result = !ShouldPreventAction(village.Owner.MapFaction);
        }
        private static bool ShouldPreventAction(IFaction otherFaction)
        {
            var playerFaction = Clan.PlayerClan.MapFaction;
            var shouldPreventAction1 = otherFaction is Kingdom encounteredKingdom && encounteredKingdom.IsRebelKingdom() && !encounteredKingdom.IsAtWarWith(playerFaction);
            var shouldPreventAction2 =
                playerFaction is Kingdom playerKingdom && playerKingdom.IsRebelKingdom() && !playerKingdom.IsAtWarWith(otherFaction);
            return shouldPreventAction1 || shouldPreventAction2;
        }

        private static void HandleThroneAbdication(Kingdom kingdom)
        {
            if (kingdom.Clans.Count <= 1 && kingdom.HasRebellion())
            {
                kingdom.GetRebelFactions().First().EnforceSuccess();
            }

            if (kingdom.Clans.Count > 1 && kingdom.HasRebellion() && kingdom.GetRebelFactions().First() is AbdicationFaction)
            {
                kingdom.GetRebelFactions().First().EnforceSuccess();
            }
        }
    }
}
