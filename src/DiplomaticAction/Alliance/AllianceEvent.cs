using TaleWorlds.CampaignSystem;

namespace Diplomacy.DiplomaticAction.Alliance
{
    class AllianceEvent
    {
        public AllianceEvent(Kingdom kingdom, Kingdom otherKingdom)
        {
            this.Kingdom = kingdom;
            this.OtherKingdom = otherKingdom;
        }

        public Kingdom Kingdom { get; }
        public Kingdom OtherKingdom { get; }
    }
}
