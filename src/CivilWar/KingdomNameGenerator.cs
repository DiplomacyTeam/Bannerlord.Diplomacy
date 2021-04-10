
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar
{
    class KingdomNameGenerator
    {
        //FIXME localization
        private static Dictionary<string, List<string>> CultureToKingdomTitles { get; } = new Dictionary<string, List<string>>()
        {
            {"empire", new() { } },
            {"aserai", new() { "Sultanate of {CLAN_NAME}", "Emirate of {CLAN_NAME}", "Banu {CLAN_NAME}", "Sharifate of {CLAN_NAME}" } },
            {"sturgia", new() { "Earldom of {CLAN_NAME}" } },
            {"vlandia", new() { "Grand Duchy of {CLAN_NAME}" } },
            {"battania", new() { "Confederation of {CLAN_NAME}" } },
            {"khuzait", new() { "Khaganate of {CLAN_NAME}", "{CLAN_NAME} Khaganate", "{CLAN_NAME} Dynasty"} },
        };

        private static List<string> CommonKingdomTitles = new()
        {
            "Principality of {CLAN_NAME}",
            "{CLAN_NAME} League",
            "{CLAN_NAME} Commune",
            "Kingdom of {CLAN_NAME}",
            "{CLAN_NAME} Empire",
            "Empire of {CLAN_NAME}",
        };


        public static TextObject GenerateKingdomName(RebelFaction rebelFaction)
        {
            string kingdomTitle;
            var culture = rebelFaction.Clans.Select(x => x.Culture.StringId).GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;
            CultureToKingdomTitles.TryGetValue(culture, out List<string> cultureTitles);
            if (cultureTitles is not null && cultureTitles.Any() && MBRandom.RandomFloat < 0.5)
            {
                kingdomTitle = cultureTitles.GetRandomElement();
            }
            else
            {
                kingdomTitle = CommonKingdomTitles.GetRandomElement();
            }

            //FIXME clan name is a terrible way to name a new kingdom

            return new TextObject(kingdomTitle, new Dictionary<string, object>() { { "CLAN_NAME", rebelFaction.SponsorClan.Name } });
        }
    }
}