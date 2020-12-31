
using Diplomacy.DiplomaticAction.GenericConditions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction
{
    abstract class AbstractConditionEvaluator<T> where T : AbstractConditionEvaluator<T>, new()
    {
        private List<IDiplomacyCondition> AllConditions { get; }
        public static T Instance { get; } = new T();

        public AbstractConditionEvaluator(List<IDiplomacyCondition> conditions)
        {
            AllConditions = new List<IDiplomacyCondition>();
            AllConditions.Add(new HasAuthorityCondition());
            AllConditions.AddRange(conditions);
        }

        public bool CanApply(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false, bool bypassCosts = false)
        {
            return AllConditions.Select(condition => condition.ApplyCondition(kingdom, otherKingdom, out TextObject textObject, forcePlayerCharacterCosts, bypassCosts)).All(x => x);
        }

        public List<TextObject> CanApplyExceptions(KingdomDiplomacyItemVM item, bool forcePlayerCharacterCosts = true, bool bypassCosts = false)
        {
            List<TextObject> textObjects = AllConditions.Select((condition) =>
            {
                condition.ApplyCondition(item.Faction1 as Kingdom, item.Faction2 as Kingdom, out TextObject textObject, forcePlayerCharacterCosts, bypassCosts);
                return textObject;
            }).OfType<TextObject>().ToList();
            return textObjects;
        }
    }
}
