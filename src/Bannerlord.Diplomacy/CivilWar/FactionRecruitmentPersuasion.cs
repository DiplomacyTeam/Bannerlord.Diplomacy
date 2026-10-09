using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.Library;

namespace Diplomacy.CivilWar
{
    internal static class FactionRecruitmentPersuasion
    {
        private static Hero? _target;
        private static PersuasionOptionArgs[]? _arguments;

        public static bool IsActive => _target is not null;

        public static PersuasionOptionArgs[]? Arguments => _arguments;

        public static void Begin(Hero target, PersuasionOptionArgs[] arguments)
        {
            _target = target;
            _arguments = arguments;
        }

        public static void End()
        {
            _target = null;
            _arguments = null;
        }

        public static bool TryGetRelationship(PersuasionOptionArgs argument, out float relation)
        {
            relation = 0f;
            if (_target is null || Hero.OneToOneConversationHero != _target
                || _arguments is null || !_arguments.Any(a => ReferenceEquals(a, argument)))
                return false;
            relation = _target.GetRelationWithPlayer();
            return true;
        }

        public static float GetRelationshipEffect(float relation, float maximumEffectInPercent)
            => MBMath.ClampFloat(relation, -100f, 100f) / 100f * MBMath.ClampFloat(maximumEffectInPercent, 0f, 100f) / 100f;

        public static void ApplyRelationshipEffect(float relation, float maximumEffectInPercent,
            ref float success, ref float criticalSuccess, ref float criticalFailure, ref float failure)
        {
            var effect = GetRelationshipEffect(relation, maximumEffectInPercent);
            if (effect == 0f)
                return;

            var originalSuccess = success + criticalSuccess;
            var adjustedSuccess = MBMath.ClampFloat(originalSuccess + effect, 0f, 1f);
            if (originalSuccess > 0f)
            {
                var factor = adjustedSuccess / originalSuccess;
                success *= factor;
                criticalSuccess *= factor;
            }
            else
            {
                success = adjustedSuccess;
                criticalSuccess = 0f;
            }

            // Preserve the previous model's split between ordinary and critical results.
            var originalFailure = failure + criticalFailure;
            if (originalFailure > 0f)
            {
                var factor = (1f - adjustedSuccess) / originalFailure;
                failure *= factor;
                criticalFailure *= factor;
            }
            else
            {
                failure = 1f - adjustedSuccess;
                criticalFailure = 0f;
            }
        }
    }
}