using DiplomacyFixes.WarPeace.Conditions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Localization;

namespace DiplomacyFixes.WarPeace
{
    class WarAndPeaceConditions
    {
        private static List<IDiplomacyCondition> _peaceConditions = new List<IDiplomacyCondition>
        {
            new SatisfiesQuestConditionsForPeaceCondition(),
            new HasEnoughGoldForPeaceCondition(),
            new HasEnoughInfluenceForPeaceCondition(),
            new HasEnoughTimeElapsedForPeaceCondition()
        };

        private static List<IDiplomacyCondition> _warConditions = new List<IDiplomacyCondition>
        {
            new HasEnoughInfluenceForWarCondition(),
            new HasLowWarExhaustionCondition(),
            new DeclareWarCooldownCondition()
        };

        public static List<TextObject> CanMakePeaceExceptions(KingdomWarItemVM item)
        {
            return CanMakePeaceExceptions(item.Faction1 as Kingdom, item.Faction2 as Kingdom, true);
        }

        private static List<TextObject> CanMakePeaceExceptions(Kingdom kingdomMakingPeace, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            return _peaceConditions.Select((condition) =>
            {
                condition.ApplyCondition(kingdomMakingPeace, otherKingdom, out TextObject textObject, forcePlayerCharacterCosts);
                return textObject;
            }).OfType<TextObject>().ToList();
        }

        public static List<TextObject> CanDeclareWarExceptions(KingdomTruceItemVM item)
        {
            return CanDeclareWarExceptions(item.Faction1 as Kingdom, item.Faction2 as Kingdom, true);
        }

        private static List<TextObject> CanDeclareWarExceptions(Kingdom kingdomDeclaringWar, Kingdom otherKingdom, bool forcePlayerCharacterCosts = false)
        {
            return _warConditions.Select((condition) =>
            {
                condition.ApplyCondition(kingdomDeclaringWar, otherKingdom, out TextObject textObject, forcePlayerCharacterCosts);
                return textObject;
            }).OfType<TextObject>().ToList();
        }

        public static bool CanProposePeace(Kingdom kingdomProposingPeace, Kingdom otherKingdom)
        {
            return _peaceConditions.Select(condition => condition.ApplyCondition(kingdomProposingPeace, otherKingdom, out _)).All(x => x);
        }

        public static bool CanDeclareWar(Kingdom kingdomDeclaringWar, Kingdom otherKingdom)
        {
            return _warConditions.Select(condition => condition.ApplyCondition(kingdomDeclaringWar, otherKingdom, out _)).All(x => x);
        }
    }
}
