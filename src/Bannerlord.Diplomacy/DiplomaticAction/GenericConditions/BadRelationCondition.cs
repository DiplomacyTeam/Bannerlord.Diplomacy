using Diplomacy.Extensions;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.GenericConditions
{
    internal sealed class BadRelationCondition : IDiplomacyCondition
    {
        public bool ApplyCondition(Kingdom kingdom,
                                   Kingdom otherKingdom,
                                   out TextObject? textObject,
                                   bool forcePlayerCosts = false,
                                   bool bypassCosts = false)
        {
            textObject = null;

            // if kingdom leaders are friends, do not allow war
            if (Settings.Instance!.NoWarBetweenFriends && kingdom.Leader.IsFriend(otherKingdom.Leader))
            {
                textObject = new TextObject("{=9aaZ8Ed4}Cannot declare war, kingdom leaders are friends.");
                return false;
            }

            // if relations are above "good" do not start wars
            if (Settings.Instance!.NoWarOnGoodRelations && kingdom.Leader.GetRelation(otherKingdom.Leader) >= Settings.Instance!.NoWarOnGoodRelationsThreshold)
            {
                textObject = new TextObject("{=Zimavbgw}Cannot declare war, kingdom leaders personal relations are excellent.");
                return false;
            }

            // if leading clans have marriages between them => no wars
            if (Settings.Instance!.NoWarWhenMarriedLeaderClans && kingdom.RulingClan.HasMarriedClanLeaderRelation(otherKingdom.RulingClan))
            {
                textObject = new TextObject("{=pXD8Uf2e}Cannot declare war, kingdom leader clans have close family marriages.");
                return false;
            }

            return true;
        }
    }
}