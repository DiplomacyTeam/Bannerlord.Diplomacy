using Helpers;

using System;

using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar
{
    internal static class FactionNameInquiry
    {
        private static readonly TextObject _TNameFactionTitle = new("{=FNnameTtl}Name Your Faction");
        private static readonly TextObject _TNameFactionText = new("{=FNnameTxt}Choose the name your faction will be known by.");

        public static void ShowForFaction(TextObject currentName, Action<TextObject> onNamed)
        {
            Show(_TNameFactionTitle, _TNameFactionText, currentName, onNamed, canCancel: true);
        }

        // Asks the player for a name, starting from the current one. Keeping the current name returns the
        // original TextObject so a generated name stays localized.
        public static void Show(TextObject title, TextObject description, TextObject currentName, Action<TextObject> onNamed, bool canCancel)
        {
            var currentText = currentName.ToString();
            InformationManager.ShowTextInquiry(new TextInquiryData(
                title.ToString(),
                description.ToString(),
                true,
                canCancel,
                GameTexts.FindText("str_ok").ToString(),
                canCancel ? GameTexts.FindText("str_cancel").ToString() : null,
                input => onNamed(input == currentText ? currentName : new TextObject("{=!}" + input)),
                null,
                false,
                // A generated name may not pass the game's rules once translated, so it is always accepted.
                input => input == currentText ? new Tuple<bool, string>(true, string.Empty) : FactionHelper.IsKingdomNameApplicable(input),
                "",
                currentText), true);
        }
    }
}
