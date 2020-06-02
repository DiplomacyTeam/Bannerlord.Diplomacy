using DiplomacyFixes.Alliance.Conditions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Localization;

namespace DiplomacyFixes.Alliance
{
    class AllianceConditions
    {
        private static List<IDiplomacyCondition> _declareAllianceConditions = new List<IDiplomacyCondition>
        {
            new AlliancesEnabledCondition(),
            new AtPeaceCondition(),
            new TimeElapsedSinceLastWarCondition(),
            new NotAlreadyInAllianceCondition(),
            new HasEnoughInfluenceCondition()
        };

        private static List<IDiplomacyCondition> _breakAllianceConditions = new List<IDiplomacyCondition>
        {
            new TimeElapsedSinceAllianceFormedCondition()
        };

        public static bool CanFormAlliance(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            return _declareAllianceConditions.Select(condition => condition.ApplyCondition(kingdom, otherKingdom, out TextObject textObject, forcePlayerCharacterCosts)).All(x => x);
        }

        public static List<TextObject> CanFormAllianceExceptions(KingdomTruceItemVM item, bool forcePlayerCharacterCosts = false)
        {
            List<TextObject> textObjects = _declareAllianceConditions.Select((condition) =>
            {
                condition.ApplyCondition(item.Faction1 as Kingdom, item.Faction2 as Kingdom, out TextObject textObject, forcePlayerCharacterCosts);
                return textObject;
            }).OfType<TextObject>().ToList();

            if (AllianceScoringModel.GetFormAllianceScore(item.Faction2 as Kingdom, item.Faction1 as Kingdom) < AllianceScoringModel.FormAllianceScoreThreshold)
            {
                TextObject scoreTooLow = new TextObject("{=VvTTrRpl}This faction is not interested in forming an alliance with you.");
                textObjects.Add(scoreTooLow);
            }
            return textObjects;
        }

        public static bool CanBreakAlliance(Kingdom kingdom, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            return _breakAllianceConditions.Select(condition => condition.ApplyCondition(kingdom, otherKingdom, out TextObject textObject, forcePlayerCharacterCosts)).All(x => x);
        }

        public static List<TextObject> CanBreakAllianceExceptions(KingdomTruceItemVM item, bool forcePlayerCharacterCosts = false)
        {
            return _breakAllianceConditions.Select((condition) =>
            {
                condition.ApplyCondition(item.Faction1 as Kingdom, item.Faction2 as Kingdom, out TextObject textObject, forcePlayerCharacterCosts);
                return textObject;
            }).OfType<TextObject>().ToList();
        }
    }
}
