using Diplomacy;
using Diplomacy.CivilWar;
using Diplomacy.Models;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.GameComponents;

using Xunit;

namespace Recruitment.Tests
{
    public sealed class RelationshipPersuasionTests : IDisposable
    {
        private readonly Hero _target = new();
        private readonly PersuasionOptionArgs _argument = new();
        private readonly DefaultPersuasionModel _previous = new();
        private readonly DiplomacyPersuasionModel _model;

        public RelationshipPersuasionTests()
        {
            Settings.Instance = new();
            Hero.OneToOneConversationHero = _target;
            FactionRecruitmentPersuasion.Begin(_target, new[] { _argument });
            _model = new(_previous);
        }

        public void Dispose()
        {
            FactionRecruitmentPersuasion.End();
            Hero.OneToOneConversationHero = null;
        }

        private (float Success, float CriticalSuccess, float CriticalFailure, float Failure) GetChances(PersuasionOptionArgs? argument = null)
        {
            _model.GetChances(argument ?? _argument, out var success, out var criticalSuccess, out var criticalFailure, out var failure, 70);
            return (success, criticalSuccess, criticalFailure, failure);
        }

        [Theory]
        [InlineData(-100, .45f)]
        [InlineData(-50, .575f)]
        [InlineData(0, .7f)]
        [InlineData(50, .825f)]
        [InlineData(100, .95f)]
        public void RelationshipChangesTheCombinedChanceOfSuccess(float relation, float expected)
        {
            _target.RelationWithPlayer = relation;
            var chances = GetChances();
            Assert.Equal(expected, chances.Success + chances.CriticalSuccess, 5);
            Assert.Equal(1, chances.Success + chances.CriticalSuccess + chances.CriticalFailure + chances.Failure, 5);
            Assert.Equal(70, _previous.LastDifficultyMultiplier);
            Assert.Equal(6, chances.Success / chances.CriticalSuccess, 5);
            Assert.Equal(2, chances.Failure / chances.CriticalFailure, 5);
        }

        [Theory]
        [InlineData(100, 100, 1)]
        [InlineData(-100, 100, 0)]
        [InlineData(1000, 25, .95f)]
        [InlineData(-1000, 25, .45f)]
        [InlineData(100, 1000, 1)]
        public void ExtremeSettingsCannotCreateInvalidProbabilityDistributions(float relation, int effect, float expected)
        {
            _target.RelationWithPlayer = relation;
            Settings.Instance.FactionRecruitmentRelationshipEffect = effect;
            var chances = GetChances();
            Assert.Equal(expected, chances.Success + chances.CriticalSuccess, 5);
            foreach (var value in new[] { chances.Success, chances.CriticalSuccess, chances.CriticalFailure, chances.Failure })
                Assert.InRange(value, 0, 1);
            Assert.Equal(1, chances.Success + chances.CriticalSuccess + chances.CriticalFailure + chances.Failure, 5);
        }

        [Theory]
        [InlineData("effect disabled")]
        [InlineData("recruitment disabled")]
        [InlineData("zero effect")]
        [InlineData("neutral relation")]
        [InlineData("different argument")]
        [InlineData("different conversation")]
        [InlineData("no conversation")]
        [InlineData("ended conversation")]
        public void UnrelatedOrDisabledPersuasionKeepsThePreviousModelsExactChances(string condition)
        {
            _target.RelationWithPlayer = 100;
            var argument = _argument;
            switch (condition)
            {
                case "effect disabled": Settings.Instance.EnableFactionRecruitmentRelationshipEffect = false; break;
                case "recruitment disabled": Settings.Instance.EnablePlayerFactionRecruitment = false; break;
                case "zero effect": Settings.Instance.FactionRecruitmentRelationshipEffect = 0; break;
                case "neutral relation": _target.RelationWithPlayer = 0; break;
                case "different argument": argument = new(); break;
                case "different conversation": Hero.OneToOneConversationHero = new(); break;
                case "no conversation": Hero.OneToOneConversationHero = null; break;
                case "ended conversation": FactionRecruitmentPersuasion.End(); break;
            }
            Assert.Equal(_previous.Chances, GetChances(argument));
        }

        [Theory]
        [InlineData(0, 50, .125f)]
        [InlineData(1, -50, .875f)]
        public void RelationshipsCanAdjustPreviouslyImpossibleOrGuaranteedRollsWithoutDivisionByZero(float original, float relation, float expected)
        {
            _previous.Chances = (original, 0, 0, 1 - original);
            _target.RelationWithPlayer = relation;
            var chances = GetChances();
            Assert.Equal(expected, chances.Success, 5);
            Assert.Equal(1 - expected, chances.Failure, 5);
            Assert.Equal(0, chances.CriticalSuccess);
            Assert.Equal(0, chances.CriticalFailure);
        }

        [Fact]
        public void RelationshipIsReadAgainWhenTheActualRollIsCalculated()
        {
            _target.RelationWithPlayer = 100;
            var preview = GetChances();
            _target.RelationWithPlayer = -100;
            var actual = GetChances();
            Assert.True(preview.Success > actual.Success);
            Assert.Equal(.45f, actual.Success + actual.CriticalSuccess, 5);
        }

        [Fact]
        public void OtherPersuasionRulesDelegateToThePreviouslyRegisteredModel()
        {
            Assert.Equal(14, _model.GetSkillXpFromPersuasion(PersuasionDifficulty.Medium, 3));
            _model.GetEffectChances(_argument, out var advance, out var block, 70);
            Assert.Equal(.7f, advance);
            Assert.Equal(.2f, block);
            Assert.Equal(PersuasionArgumentStrength.Normal,
                _model.GetArgumentStrengthBasedOnTargetTraits(new CharacterObject(), Array.Empty<Tuple<TraitObject, int>>()));
            Assert.Equal(70, _model.GetDifficulty(PersuasionDifficulty.Medium));
            Assert.Equal(73, _model.CalculateInitialPersuasionProgress(new CharacterObject(), 3, 70));
            Assert.Equal(6, _model.CalculatePersuasionGoalValue(new CharacterObject(), 3));
        }
    }
}