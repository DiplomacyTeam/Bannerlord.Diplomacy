using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.WarPeace.Conditions
{
    internal class NoPlayerSiegeCondition : IDiplomacyCondition
    {

        private static readonly TextObject _TActivePlayerSiege = new("{=XlL50Ha9}Can't make peace with a kingdom during an active player siege.");

        public bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
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