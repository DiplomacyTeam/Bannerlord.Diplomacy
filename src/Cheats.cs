using Diplomacy.DiplomaticAction.Alliance;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace Diplomacy.Extensions
{
    class CampaignCheatsExtension
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("form_alliance", "campaign")]
        public static string FormAlliance(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            if (!CampaignCheats.CheckParameters(strings, 2) || CampaignCheats.CheckHelp(strings))
                return "Format uses kingdom IDs without spaces: \"campaign.form_alliance [Kingdom1] [Kingdom2]\".";

            var b1 = strings[0].ToLower();
            var b2 = strings[1].ToLower();

            if (b1 == b2)
                return "Cannot ally a kingdom to itself!";

            Kingdom? kingdom1 = null;
            Kingdom? kingdom2 = null;

            foreach (var k in Kingdom.All)
            {
                var id = k.Name.ToString().ToLower().Replace(" ", "");

                if (id == b1)
                    kingdom1 = k;
                else if (id == b2)
                    kingdom2 = k;
            }

            if (kingdom1 is null)
                return "1st kingdom ID not found: " + b1;

            if (kingdom2 is null)
                return "2nd kingdom ID not found: " + b2;

            DeclareAllianceAction.Apply(kingdom1, kingdom2, bypassCosts: true);
            return $"Alliance formed between {kingdom1.Name} and  {kingdom2.Name}";
        }
    }
}
