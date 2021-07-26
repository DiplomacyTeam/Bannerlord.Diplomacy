using TaleWorlds.CampaignSystem;

namespace Diplomacy.Event
{
    internal readonly struct WarExhaustionEvent
    {
        public WarExhaustionEvent(Kingdom kingdom, Kingdom otherKingdom, WarExhaustionManager.WarExhaustionType warExhaustionType, float warExhaustionToAdd)
        {
            Kingdom = kingdom;
            OtherKingdom = otherKingdom;
            WarExhaustionType = warExhaustionType;
            WarExhaustionToAdd = warExhaustionToAdd;
        }

        public Kingdom Kingdom { get; }
        public Kingdom OtherKingdom { get; }
        public WarExhaustionManager.WarExhaustionType WarExhaustionType { get; }
        public float WarExhaustionToAdd { get; }
    }
}
