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
#if v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115 || v116
                var besiegingKingdom = PlayerSiege.PlayerSiegeEvent?.BesiegerCamp?.BesiegerParty.LeaderHero.MapFaction as Kingdom;
#else
                var besiegingKingdom = PlayerSiege.PlayerSiegeEvent?.BesiegerCamp?.LeaderParty.LeaderHero.MapFaction as Kingdom;
#endif

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