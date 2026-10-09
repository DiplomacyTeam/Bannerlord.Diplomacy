namespace TaleWorlds.CampaignSystem.ComponentInterfaces
{
    using CharacterDevelopment;
    using Conversation.Persuasion;

    public abstract class PersuasionModel
    {
        public abstract int GetSkillXpFromPersuasion(PersuasionDifficulty difficulty, int argumentDifficulty);
        public abstract void GetChances(PersuasionOptionArgs optionArgs, out float success, out float criticalSuccess,
            out float criticalFailure, out float failure, float difficultyMultiplier);
        public abstract void GetEffectChances(PersuasionOptionArgs optionArgs, out float moveToNextStageChance,
            out float blockOtherOptionChance, float difficultyMultiplier);
        public abstract PersuasionArgumentStrength GetArgumentStrengthBasedOnTargetTraits(CharacterObject character, Tuple<TraitObject, int>[] traitCorrelations);
        public abstract float GetDifficulty(PersuasionDifficulty difficulty);
        public abstract float CalculateInitialPersuasionProgress(CharacterObject character, float targetPersuasionScore, float difficultyMultiplier);
        public abstract float CalculatePersuasionGoalValue(CharacterObject character, float baseGoalValue);
    }
}
