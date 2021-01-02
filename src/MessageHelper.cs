using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy
{
    class MessageHelper
    {
        public static void SendFailedActionMessage(string action, List<string> exceptions)
        {
            var sb = new StringBuilder();
            sb.Append(action);
            sb.Append(exceptions.First());

            InformationManager.DisplayMessage(new InformationMessage(sb.ToString()));
        }

        public static void SendFailedActionMessage(string action, List<TextObject> exceptions)
        {
            var sb = new StringBuilder();
            sb.Append(action);
            sb.Append(exceptions.First().ToString());

            InformationManager.DisplayMessage(new InformationMessage(sb.ToString()));
        }
    }
}
