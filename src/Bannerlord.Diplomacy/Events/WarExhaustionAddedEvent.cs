using Diplomacy.WarExhaustion;

using TaleWorlds.CampaignSystem;

namespace Diplomacy.Events
{
    public readonly struct WarExhaustionAddedEvent
    {
        public WarExhaustionAddedEvent(Kingdom kingdom, Kingdom otherKingdom, WarExhaustionType warExhaustionType, float warExhaustionToAdd)
        {
            Kingdom = kingdom;
            OtherKingdom = otherKingdom;
            WarExhaustionType = warExhaustionType;
            WarExhaustionToAdd = warExhaustionToAdd;
        }

        public Kingdom Kingdom { get; }
        public Kingdom OtherKingdom { get; }
        public WarExhaustionType WarExhaustionType { get; }
        public float WarExhaustionToAdd { get; }
    }
}