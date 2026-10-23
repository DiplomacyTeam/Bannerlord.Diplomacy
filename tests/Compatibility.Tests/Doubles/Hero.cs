using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem
{
    public sealed class Hero
    {
        public static Hero MainHero { get; set; } = new();
        public string StringId { get; init; } = "test_hero";
        public string Name { get; init; } = "Test hero";
        public CultureObject Culture { get; init; } = new();
        public bool IsActive { get; set; } = true;
        public Equipment BattleEquipment { get; } = new();
        public Equipment CivilianEquipment { get; } = new() { IsCivilian = true };
    }
}
