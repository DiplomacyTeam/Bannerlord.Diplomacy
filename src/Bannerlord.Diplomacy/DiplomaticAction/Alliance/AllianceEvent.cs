using TaleWorlds.CampaignSystem;

namespace Diplomacy.DiplomaticAction.Alliance
{
    public readonly struct AllianceEvent
    {
        public AllianceEvent(Kingdom kingdom, Kingdom otherKingdom)
        {
            Kingdom = kingdom;
            OtherKingdom = otherKingdom;
        }

        public Kingdom Kingdom { get; }
        public Kingdom OtherKingdom { get; }
    }
}