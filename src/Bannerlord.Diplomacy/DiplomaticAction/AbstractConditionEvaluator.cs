using Diplomacy.DiplomaticAction.GenericConditions;
using Diplomacy.Extensions;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Localization;

namespace Diplomacy.DiplomaticAction
{
    internal abstract class AbstractConditionEvaluator<T> where T : AbstractConditionEvaluator<T>, new()
    {
        private List<AbstractDiplomacyCondition> Conditions { get; }

        public static T Instance { get; } = new();

        protected AbstractConditionEvaluator(List<AbstractDiplomacyCondition> conditions)
        {
            Conditions = new List<AbstractDiplomacyCondition> { new HasAuthorityCondition() };
            Conditions.AddRange(conditions);
        }

        public bool CanApply(Kingdom kingdom, Kingdom otherKingdom, DiplomacyConditionType accountedConditions = DiplomacyConditionType.All,
                             bool forcePlayerCharacterCosts = false, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            // FIXME: Simplified LINQ here, but it also had the effect of not executing ApplyCondition on some conditions if an early one failed.
            // So if there are conditions with side effects that should still execute even upon failure, this needs to be changed.
            return Conditions.All(c => c.ApplyCondition(kingdom, otherKingdom, out _, accountedConditions, forcePlayerCharacterCosts, kingdomPartyType));
        }

        public List<TextObject> CanApplyExplained(KingdomDiplomacyItemVM item, DiplomacyConditionType accountedConditions = DiplomacyConditionType.All,
                                                  bool forcePlayerCharacterCosts = true, DiplomaticPartyType kingdomPartyType = DiplomaticPartyType.Proposer)
        {
            return Conditions.Select(IssuesSelector).OfType<TextObject>().ToList();

            TextObject? IssuesSelector(AbstractDiplomacyCondition condition)
            {
                switch (condition.ConditionType)
                {
                    case DiplomacyConditionType.ScoreRelated when kingdomPartyType == DiplomaticPartyType.Proposer && ((Kingdom)item.Faction1).IsRuledByPlayer():
                        {
                            condition.ApplyCondition((Kingdom)item.Faction2, (Kingdom)item.Faction1, out TextObject? txt, accountedConditions, forcePlayerCharacterCosts, kingdomPartyType);
                            return txt;
                        }
                    default:
                        {
                            condition.ApplyCondition((Kingdom)item.Faction1, (Kingdom)item.Faction2, out TextObject? txt, accountedConditions, forcePlayerCharacterCosts, kingdomPartyType);
                            return txt;
                        }
                }
            }
        }
    }
}