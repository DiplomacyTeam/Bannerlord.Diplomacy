using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

namespace TaleWorlds.Core
{
    public sealed class Equipment
    {
        public bool IsCivilian { get; init; }
        public string Outfit { get; set; } = "existing outfit";
    }
}

namespace TaleWorlds.CampaignSystem
{
    using TaleWorlds.Core;

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

    public sealed class CultureObject
    {
        public string StringId { get; init; } = "custom_culture";
    }

    public sealed class Clan
    {
        public Hero Leader { get; init; } = new();
    }

    public sealed class Kingdom
    {
        public Hero Leader { get; init; } = new();
    }
}

namespace Helpers
{
    using TaleWorlds.CampaignSystem;
    using TaleWorlds.Core;

    public static class EquipmentHelper
    {
        // Reproduces the game's unchecked dereference and observable equipment update.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AssignHeroEquipmentFromEquipment(Hero hero, Equipment equipment)
        {
            var current = equipment.IsCivilian ? hero.CivilianEquipment : hero.BattleEquipment;
            current.Outfit = equipment.Outfit;
        }
    }
}

namespace TaleWorlds.CampaignSystem.CampaignBehaviors
{
    using Helpers;
    using TaleWorlds.Core;

    public sealed class NPCEquipmentsCampaignBehavior
    {
        public Equipment? NewBattle { get; set; }
        public Equipment? NewCivilian { get; set; }
        public Equipment? OldBattle { get; set; }
        public Equipment? OldCivilian { get; set; }
        public bool Completed { get; private set; }

        public void RaiseRulingClanChanged(Kingdom kingdom, Clan? oldRulingClan)
            => OnRulingClanChanged(kingdom, oldRulingClan);

        // Exercise the real Harmony transpiler against the four assignment call sites and
        // player/inactive-ruler branches used by Bannerlord, without needing a running campaign.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void OnRulingClanChanged(Kingdom kingdom, Clan? oldRulingClan)
        {
            if (kingdom.Leader != Hero.MainHero)
            {
                EquipmentHelper.AssignHeroEquipmentFromEquipment(kingdom.Leader, NewBattle!);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(kingdom.Leader, NewCivilian!);
            }

            var oldRuler = oldRulingClan?.Leader;
            if (oldRuler is not null && oldRuler.IsActive && oldRuler != Hero.MainHero)
            {
                EquipmentHelper.AssignHeroEquipmentFromEquipment(oldRuler, OldBattle!);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(oldRuler, OldCivilian!);
            }

            Completed = true;
        }
    }
}

namespace Diplomacy
{
    internal static class LogFactory
    {
        public static List<string> Warnings { get; } = new();
        public static ILogger Get<T>() => new TestLogger();

        private sealed class TestLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => throw new NotSupportedException();
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter) => Warnings.Add(formatter(state, exception));
        }
    }
}
