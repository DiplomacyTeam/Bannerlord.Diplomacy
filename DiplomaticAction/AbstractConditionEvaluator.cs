
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction
{
    abstract class AbstractConditionEvaluator<T> where T : AbstractConditionEvaluator<T>, new()
    {
        protected abstract List<IDiplomacyCondition> Conditions { get; }
        public static T Instance { get; } = new T();

        public bool CanApply(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            return Conditions.Select(condition => condition.ApplyCondition(kingdom, otherKingdom, out TextObject textObject, forcePlayerCharacterCosts, bypassCosts)).All(x => x);
        }

        public List<TextObject> CanApplyExceptions(KingdomDiplomacyItemVM item, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            List<TextObject> textObjects = Conditions.Select((condition) =>
            {
                condition.ApplyCondition(item.Faction1 as Kingdom, item.Faction2 as Kingdom, out TextObject textObject, forcePlayerCharacterCosts, bypassCosts);
                return textObject;
            }).OfType<TextObject>().ToList();
            return textObjects;
        }
    }
}
