using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy
{
    interface IDiplomacyCondition
    {
        bool ApplyCondition(Kingdom kingdom, Kingdom otherKingdom, out TextObject? textObject, bool forcePlayerCharacterCosts = false, bool bypassCosts = false);
    }
}