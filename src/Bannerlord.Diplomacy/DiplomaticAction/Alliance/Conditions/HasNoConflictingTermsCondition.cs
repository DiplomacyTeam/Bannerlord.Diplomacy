using Bannerlord.ButterLib.Common.Helpers;

using Diplomacy.DiplomaticAction.Conditioning;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction.Alliance.Conditions
{
    internal sealed class HasNoConflictingTermsCondition : AbstractDiplomacyCondition
    {
        private static readonly TextObject _THasConflictingTerms = new("{=8rhYru1w}Has a diplomatic {?POTENTIAL_ENEMY_KINGDOMS.IS_PLURAL}agreements{?}agreement{//?} with {POTENTIAL_ENEMY_KINGDOMS} {?POTENTIAL_ENEMY_KINGDOMS.IS_PLURAL}kingdoms{?}kingdom{//?} that can not be broken.");

        protected override bool ApplyConditionInternal(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            textObject = null;
            var potentialEnemies = Kingdom.All.Where(k => FactionManager.IsAtWarAgainstFaction(k, otherKingdom) && !FactionManager.IsAtWarAgainstFaction(k, kingdom));
            var conflictedKingdoms = potentialEnemies.Where(HasBloackingTerms).Select(k => k.Name.ToString());
            bool hasConflictingTerms = conflictedKingdoms.Any();

            if (hasConflictingTerms)
            {
                textObject = _THasConflictingTerms.CopyTextObject();
                LocalizationHelper.SetListVariable(textObject, "POTENTIAL_ENEMY_KINGDOMS", conflictedKingdoms.ToList());
            }
            return !hasConflictingTerms;

            bool HasBloackingTerms(Kingdom potentialEnemy) =>
                (FactionManager.IsAlliedWithFaction(kingdom, potentialEnemy) && !BreakAllianceAction.CanApply(kingdom, potentialEnemy))
                || DiplomaticAgreementManager.HasNonAggressionPact(kingdom, potentialEnemy, out _);
        }
    }
}
