namespace TaleWorlds.CampaignSystem.GameComponents
{
    using CharacterDevelopment;
    using ComponentInterfaces;
    using Conversation.Persuasion;

    // Deterministic stand-in for a previously registered persuasion model, not a simulation of native Charm.
    public class DefaultPersuasionModel : PersuasionModel
    {
        public (float Success, float CriticalSuccess, float CriticalFailure, float Failure) Chances { get; set; } = (.6f, .1f, .1f, .2f);
        public float LastDifficultyMultiplier { get; private set; }

        public override void GetChances(PersuasionOptionArgs optionArgs, out float success, out float criticalSuccess,
            out float criticalFailure, out float failure, float difficultyMultiplier)
        {
            LastDifficultyMultiplier = difficultyMultiplier;
            (success, criticalSuccess, criticalFailure, failure) = Chances;
        }

        public override int GetSkillXpFromPersuasion(PersuasionDifficulty difficulty, int argumentDifficulty) => argumentDifficulty + 11;

        public override void GetEffectChances(PersuasionOptionArgs optionArgs, out float moveToNextStageChance,
            out float blockOtherOptionChance, float difficultyMultiplier)
        {
            moveToNextStageChance = difficultyMultiplier / 100;
            blockOtherOptionChance = .2f;
        }

        public override PersuasionArgumentStrength GetArgumentStrengthBasedOnTargetTraits(CharacterObject character, Tuple<TraitObject, int>[] traitCorrelations)
            => PersuasionArgumentStrength.Normal;

        public override float GetDifficulty(PersuasionDifficulty difficulty) => 70;

        public override float CalculateInitialPersuasionProgress(CharacterObject character, float targetPersuasionScore, float difficultyMultiplier)
            => targetPersuasionScore + difficultyMultiplier;

        public override float CalculatePersuasionGoalValue(CharacterObject character, float baseGoalValue) => baseGoalValue * 2;
    }
}
