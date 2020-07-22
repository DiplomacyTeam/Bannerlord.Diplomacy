using DiplomacyFixes.DiplomaticAction.Alliance;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace DiplomacyFixes.Extensions
{
    class CampaignCheatsExtension
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("form_alliance", "campaign")]
        public static string FormAlliance(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
            {
                return CampaignCheats.ErrorType;
            }
            if (!CampaignCheats.CheckParameters(strings, 2) || CampaignCheats.CheckHelp(strings))
            {
                return "Format is faction names without space \"campaign.form_alliance [Faction1] [Faction2]\".";
            }
            string b = strings[0].ToLower();
            string b2 = strings[1].ToLower();
            Kingdom faction = null;
            Kingdom faction2 = null;
            foreach (Kingdom faction3 in Campaign.Current.Kingdoms)
            {
                string a = faction3.Name.ToString().ToLower().Replace(" ", "");
                if (a == b)
                {
                    faction = faction3;
                }
                else if (a == b2)
                {
                    faction2 = faction3;
                }
            }
            if (faction != null && faction2 != null)
            {
                DeclareAllianceAction.Apply(faction as Kingdom, faction2 as Kingdom, bypassCosts: true);
                return string.Concat(new object[]
                {
                    "Alliance declared between ",
                    faction.Name,
                    " and ",
                    faction2.Name
                });
            }
            if (faction == null)
            {
                return "Faction is not found: " + faction;
            }
            return "Faction is not found: " + faction2;
        }
    }
}
