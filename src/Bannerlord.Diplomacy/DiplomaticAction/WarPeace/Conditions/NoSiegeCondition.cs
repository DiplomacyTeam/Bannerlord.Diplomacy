using Diplomacy.DiplomaticAction.Conditioning;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace.Conditions
{
    internal class NoPlayerSiegeCondition : AbstractDiplomacyCondition
    {
        private static readonly TextObject _TActivePlayerSiege = new("{=XlL50Ha9}Can't make peace with a kingdom during an active player siege.");

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            textObject = null;
            var playerKingdom = Clan.PlayerClan.Kingdom;
            if (kingdom == playerKingdom || otherKingdom == playerKingdom)
            {
                var besiegedKingdom = PlayerSiege.PlayerSiegeEvent?.BesiegedSettlement?.OwnerClan.Kingdom;
                var besiegingKingdom = PlayerSiege.PlayerSiegeEvent?.BesiegerCamp?.BesiegerParty.LeaderHero.MapFaction as Kingdom;

                if ((besiegedKingdom == playerKingdom || besiegingKingdom == playerKingdom) && (besiegedKingdom == otherKingdom || besiegingKingdom == otherKingdom))
                {
                    textObject = _TActivePlayerSiege;
                    return false;
                }
            }
            return true;
        }
    }
}