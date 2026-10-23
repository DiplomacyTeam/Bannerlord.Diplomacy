using Diplomacy;
using Diplomacy.Patches;

using HarmonyLib;

using Helpers;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.Core;

using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Compatibility.Tests
{
    public sealed class RulerEquipmentTests : IDisposable
    {
        private readonly Harmony _harmony = new("diplomacy.tests.ruler-equipment");
        private readonly NPCEquipmentsCampaignBehaviorPatch _patch = new();
        private readonly Kingdom _kingdom = new();
        private readonly Clan _oldClan = new() { Leader = new() { StringId = "old_ruler" } };
        private readonly NPCEquipmentsCampaignBehavior _behavior = new()
        {
            NewBattle = new() { Outfit = "new ruler battle" },
            NewCivilian = new() { IsCivilian = true, Outfit = "new ruler civilian" },
            OldBattle = new() { Outfit = "old ruler battle" },
            OldCivilian = new() { IsCivilian = true, Outfit = "old ruler civilian" },
        };

        public RulerEquipmentTests()
        {
            Hero.MainHero = new();
            LogFactory.Warnings.Clear();
            Assert.Single(_patch.Patches).Apply(_harmony);
        }

        public void Dispose()
        {
            foreach (var patch in _patch.Patches)
                patch.Remove(_harmony);
        }

        [Theory]
        [InlineData("new battle", 1, "test_hero")]
        [InlineData("new civilian", 1, "test_hero")]
        [InlineData("old battle", 1, "old_ruler")]
        [InlineData("old civilian", 1, "old_ruler")]
        [InlineData("all", 4, null)]
        [InlineData("none", 0, null)]
        public void MissingOutfitsArePreservedAndValidOutfitsStillApply(string missing, int expectedWarnings, string? expectedHeroId)
        {
            SetMissingOutfits(missing);

            _behavior.RaiseRulingClanChanged(_kingdom, _oldClan);

            Assert.True(_behavior.Completed);
            Assert.Equal(_behavior.NewBattle?.Outfit ?? "existing outfit", _kingdom.Leader.BattleEquipment.Outfit);
            Assert.Equal(_behavior.NewCivilian?.Outfit ?? "existing outfit", _kingdom.Leader.CivilianEquipment.Outfit);
            Assert.Equal(_behavior.OldBattle?.Outfit ?? "existing outfit", _oldClan.Leader.BattleEquipment.Outfit);
            Assert.Equal(_behavior.OldCivilian?.Outfit ?? "existing outfit", _oldClan.Leader.CivilianEquipment.Outfit);
            Assert.Equal(expectedWarnings, LogFactory.Warnings.Count);
            Assert.All(LogFactory.Warnings, warning => Assert.Contains("custom_culture", warning));
            if (expectedHeroId is not null) Assert.Contains(expectedHeroId, Assert.Single(LogFactory.Warnings));
        }

        private void SetMissingOutfits(string missing)
        {
            if (missing is "new battle" or "all") _behavior.NewBattle = null;
            if (missing is "new civilian" or "all") _behavior.NewCivilian = null;
            if (missing is "old battle" or "all") _behavior.OldBattle = null;
            if (missing is "old civilian" or "all") _behavior.OldCivilian = null;
        }

        [Fact]
        public void FoundingAKingdomWithoutAnOldRulerCanCompleteWithNoTemplates()
        {
            _behavior.NewBattle = null;
            _behavior.NewCivilian = null;
            _behavior.RaiseRulingClanChanged(_kingdom, null);
            Assert.True(_behavior.Completed);
            Assert.Equal("existing outfit", _kingdom.Leader.BattleEquipment.Outfit);
            Assert.Equal("existing outfit", _kingdom.Leader.CivilianEquipment.Outfit);
            Assert.Equal(2, LogFactory.Warnings.Count);
        }

        [Theory]
        [InlineData("new player")]
        [InlineData("old player")]
        [InlineData("inactive old ruler")]
        public void PlayersAndInactiveFormerRulersKeepTheirEquipment(string scenario)
        {
            if (scenario == "new player") Hero.MainHero = _kingdom.Leader;
            if (scenario == "old player") Hero.MainHero = _oldClan.Leader;
            if (scenario == "inactive old ruler") _oldClan.Leader.IsActive = false;
            _behavior.RaiseRulingClanChanged(_kingdom, _oldClan);

            var unchangedHero = scenario == "new player" ? _kingdom.Leader : _oldClan.Leader;
            Assert.Equal("existing outfit", unchangedHero.BattleEquipment.Outfit);
            Assert.Equal("existing outfit", unchangedHero.CivilianEquipment.Outfit);
            Assert.True(_behavior.Completed);
            Assert.Empty(LogFactory.Warnings);
        }

        [Fact]
        public void EquipmentAssignmentsOutsideRulerChangesAreNotPatched()
        {
            Assert.Throws<NullReferenceException>(() => EquipmentHelper.AssignHeroEquipmentFromEquipment(_kingdom.Leader, null!));
            EquipmentHelper.AssignHeroEquipmentFromEquipment(_kingdom.Leader, new Equipment { Outfit = "unrelated outfit" });
            Assert.Equal("unrelated outfit", _kingdom.Leader.BattleEquipment.Outfit);
            Assert.Empty(LogFactory.Warnings);
        }

        [Fact]
        public void TheUnpatchedHandlerReproducesTheReportedNullEquipmentCrash()
        {
            foreach (var patch in _patch.Patches)
                patch.Remove(_harmony);
            _behavior.NewBattle = null;
            Assert.Throws<NullReferenceException>(() => _behavior.RaiseRulingClanChanged(_kingdom, null));
            Assert.False(_behavior.Completed);
        }
    }
}
