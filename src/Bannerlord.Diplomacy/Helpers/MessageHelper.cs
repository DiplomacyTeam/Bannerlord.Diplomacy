using System.Collections.Generic;
using System.Linq;
using System.Text;

using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.Helpers
{
    internal static class MessageHelper
    {
        public static void SendFailedActionMessage(string action, List<string> exceptions)
        {
            var sb = new StringBuilder();
            sb.Append(action);
            if (exceptions.Any()) sb.Append(exceptions.First());

            InformationManager.DisplayMessage(new InformationMessage(sb.ToString()));
        }

        public static void SendFailedActionMessage(string action, List<TextObject> exceptions)
        {
            var sb = new StringBuilder();
            sb.Append(action);
            if (exceptions.Any()) sb.Append(exceptions.First());

            InformationManager.DisplayMessage(new InformationMessage(sb.ToString()));
        }
    }
}