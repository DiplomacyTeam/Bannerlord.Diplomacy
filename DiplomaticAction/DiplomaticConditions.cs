using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction
{
    abstract class DiplomaticConditions
    {
        protected static DiplomaticConditions _instance;

        private List<IDiplomacyCondition> _conditions;

        public DiplomaticConditions(List<IDiplomacyCondition> conditions)
        {
            this._conditions = conditions;
        }

        public bool CanExecuteAction(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            return _conditions.Select(condition => condition.ApplyCondition(kingdom, otherKingdom, out TextObject textObject, forcePlayerCharacterCosts)).All(x => x);
        }

        public List<TextObject> CanExecuteActionExceptions(KingdomDiplomacyItemVM item, bool forcePlayerCharacterCosts = false)
        {
            List<TextObject> textObjects = _conditions.Select((condition) =>
            {
                condition.ApplyCondition(item.Faction1 as Kingdom, item.Faction2 as Kingdom, out TextObject textObject, forcePlayerCharacterCosts);
                return textObject;
            }).OfType<TextObject>().ToList();

            return textObjects;
        }

    }
}
