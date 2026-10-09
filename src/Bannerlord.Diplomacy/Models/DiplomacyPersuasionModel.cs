using Diplomacy.CivilWar;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.GameComponents;

namespace Diplomacy.Models
{
    internal sealed class DiplomacyPersuasionModel : PersuasionModel
    {
        private readonly PersuasionModel _previousModel;

        public DiplomacyPersuasionModel(PersuasionModel? previousModel)
            => _previousModel = previousModel ?? new DefaultPersuasionModel();

        public override void GetChances(PersuasionOptionArgs optionArgs, out float successChance, out float critSuccessChance,
            out float critFailChance, out float failChance, float difficultyMultiplier)
        {
            _previousModel.GetChances(optionArgs, out successChance, out critSuccessChance, out critFailChance, out failChance, difficultyMultiplier);
            if (Settings.Instance!.EnablePlayerFactionRecruitment && Settings.Instance!.EnableFactionRecruitmentRelationshipEffect
                && FactionRecruitmentPersuasion.TryGetRelationship(optionArgs, out var relation))
                FactionRecruitmentPersuasion.ApplyRelationshipEffect(relation, Settings.Instance!.FactionRecruitmentRelationshipEffect,
                    ref successChance, ref critSuccessChance, ref critFailChance, ref failChance);
        }

        public override int GetSkillXpFromPersuasion(PersuasionDifficulty difficulty, int argumentDifficulty)
            => _previousModel.GetSkillXpFromPersuasion(difficulty, argumentDifficulty);

        public override void GetEffectChances(PersuasionOptionArgs optionArgs, out float moveToNextStageChance, out float blockOtherOptionChance, float difficultyMultiplier)
            => _previousModel.GetEffectChances(optionArgs, out moveToNextStageChance, out blockOtherOptionChance, difficultyMultiplier);

        public override PersuasionArgumentStrength GetArgumentStrengthBasedOnTargetTraits(CharacterObject character, Tuple<TraitObject, int>[] traitCorrelations)
            => _previousModel.GetArgumentStrengthBasedOnTargetTraits(character, traitCorrelations);

        public override float GetDifficulty(PersuasionDifficulty difficulty) => _previousModel.GetDifficulty(difficulty);

        public override float CalculateInitialPersuasionProgress(CharacterObject character, float targetPersuasionScore, float difficultyMultiplier)
            => _previousModel.CalculateInitialPersuasionProgress(character, targetPersuasionScore, difficultyMultiplier);

        public override float CalculatePersuasionGoalValue(CharacterObject character, float baseGoalValue)
            => _previousModel.CalculatePersuasionGoalValue(character, baseGoalValue);
    }
}