using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplomacy
{
    public static class Utillities
    {
        public static string wrapFactionName(string factionName)
        {
            string wrappedFactionName;

            var prefixes = factionName.Split(' ');
            string factionNamePrefix = prefixes.Length > 1 ? prefixes[0] : "";

            var postfixes = factionName.Split('(', ')');
            string factionNamePostfix = postfixes.Length > 1 ? '(' + postfixes[1] + ')' : "";

            bool prefixExists = factionNamePrefix != "";
            bool postfixExists = factionNamePostfix != "";
            string factionNameBody = prefixExists ? factionName.Replace(factionNamePrefix + ' ', "") : factionName;
            factionNameBody = postfixExists ? factionNameBody.Replace(' ' + factionNamePostfix, "") : factionNameBody;

            wrappedFactionName = factionNamePrefix + (prefixExists ? "\n" : "")
                            + factionNameBody + (postfixExists ? "\n" : "")
                            + factionNamePostfix;
            return wrappedFactionName;
        }
    }
}
