using Diplomacy.CivilWar.Factions;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar
{
    internal static class FactionNameGenerator
    {
        private static Dictionary<string, List<TitleSelection>> CultureToKingdomTitles { get; } = new()
        {
            // {"empire", new() { } },
            {
                "aserai",
                new()
                {
                    new("{=iLMqCvmk}Sultanate of {CLAN_NAME}"),
                    new("{=EnpnMNRH}Emirate of {CLAN_NAME}"),
                    new("{=uzd9mxIC}Banu {CLAN_NAME}"),
                    new("{=NTFz3B56}Sharifate of {CLAN_NAME}")
                }
            },
            {
                "battania",
                new()
                {
                    new(StringConstants.FactionConfederation),
                }
            },
            {
                "khuzait",
                new()
                {
                    new("{=mEHufuep}Khaganate of {CLAN_NAME}", 0.5f),
                    new("{=DTBaowBi}{CLAN_NAME} Khaganate", 0.5f),
                    new("{=pRgQF4WY}{CLAN_NAME} Dynasty")
                }
            },
            {
                "sturgia",
                new()
                {
                    new("{=z5VPFrfh}Earldom of {CLAN_NAME}")
                }
            },
            {
                "vlandia",
                new()
                {
                    new("{=A4zT7H5g}Grand Duchy of {CLAN_NAME}")
                }
            },
        };

        private static readonly List<TitleSelection> CommonKingdomTitles = new()
        {
            new(StringConstants.FactionLeague),
            new("{=kToqhmHh}Principality of {CLAN_NAME}"),
            new("{=WjB3SM1f}Kingdom of {CLAN_NAME}"),
            new("{=E63n30UM}{CLAN_NAME} Empire", 0.5f),
            new("{=lSQeaVfw}Empire of {CLAN_NAME}", 0.5f),
        };

        private static readonly List<string> FactionNames = new()
        {
            StringConstants.FactionConspiracy,
            StringConstants.FactionConfederation,
            StringConstants.FactionLeague
        };

        private static readonly Dictionary<string, List<string>> CultureToKingdomNames = new()
        {
            {
                "aserai",
                new()
                {
                    "{=j3bEr6n5}Ahmerrad",
                    "{=QBJk3ymm}Bariyye",
                    "{=qWmWT0xA}Durquba",
                    "{=X75ITMUL}Shariz",
                    "{=K7udl5FQ}Bardaq",
                    "{=OGwXFmDB}Caraf",
                    "{=I7c8WNxV}Durrin",
                    "{=M4UquaBO}Jameyyed",
                    "{=NndCJIzP}Samarra",
                    "{=tDQzTgry}Sharwa",
                    "{=FSWiNAyj}Teramma",
                    "{=PEd8JgL2}Weyyah",
                    "{=ma2FmIjQ}Aab",
                    "{=1lNXlUvm}Ayn Assuadi",
                    "{=i8y3UunL}Dhibbain",
                    "{=uOld11Db}Fishara",
                    "{=FZQG8HZ0}Habba",
                    "{=cD8ZseSp}Hawaha",
                    "{=rvsefzHq}Iqbayl",
                    "{=OOGTpFKw}Mawiti",
                    "{=wSlwO5jG}Mazigh",
                    "{=TU4Itb6v}Mijayet",
                    "{=SmIIc4YA}Mit Nun",
                    "{=mAT6Zajm}Qalyut",
                    "{=7C8DXcNT}Rushdigh",
                    "{=T9VemCVx}Sekhtem",
                    "{=zTJ3Oaki}Shibal Zumr",
                    "{=seBpcqnW}Tamnuh",
                    "{=U6enpew6}Tazjunat",
                    "{=AB8kd1kQ}Tilimsal",
                    "{=Ug4D9NN0}Unriya",
                    "{=lsMTtqa4}Uzgha",
                }
            },
            {
                "battania",
                new()
                {
                    "{=qAuhdK0v}Crovia",
                    "{=o1ORogYL}Hacia",
                    "{=rfgjbrvw}Uveagia",
                    "{=STzd8avo}Theoles",
                    "{=22NuLUbK}Achiane",
                    "{=AFlVSUFu}Rolastien",
                    "{=AIKEX745}Fraynak",
                    "{=RJczxlW4}Dorcard",
                    "{=wGO3G6yy}Wynistyr",
                    "{=q7wUF854}Uradal",
                    "{=aAZMAMx6}Dunerrice",
                    "{=qe9NrPHL}Bartoryntham",
                    "{=x67QSESS}Laniyrick",
                    "{=YzLeuoSR}Imeannel",
                    "{=OoVKShKC}Bircenan",
                }
            },
            {
                "khuzait",
                new()
                {
                    "{=59uhOyuZ}Kunait",
                    "{=KKgoCDJZ}Evrukha",
                    "{=Z141AJ5X}Bulan",
                    "{=wOrNq16t}Halmar",
                    "{=HtX8KdgW}Ichamur",
                    "{=zpmMniuA}Narra",
                    "{=xDWNd4sN}Tulga",
                    "{=DLfwRFh1}Asugan",
                    "{=UxASxuob}Distar",
                    "{=4stgAKam}Malayurg",
                    "{=04dkhDAi}Sungetche",
                    "{=ZOIt9Lb6}Tulbuk",
                    "{=MzHLDXdO}Uhhun",
                    "{=XjbYmG6p}Unuzdaq",
                    "{=vY8f12Qd}Ada Kulun",
                    "{=Blar1tNW}Amashke",
                    "{=k3pz8N53}Bhulaban",
                    "{=K7WMsExR}Bulugur",
                    "{=6xB2dREA}Dashbigha",
                    "{=cEL50k9K}Dirigh Aban",
                    "{=AJyY3SwF}Dugan",
                    "{=54CFY7Y7}Dusturil",
                    "{=XExpkOg8}Kedelke",
                    "{=sp4R5ttP}Peshmi",
                    "{=OSSt9yPb}Tash Kulun",
                    "{=uAySGYSA}Tismirr",
                    "{=0Ex3esLT}Tulbuk",
                    "{=7h6VTRli}Uhhun",
                    "{=n0GNLDVS}Zagush",
                }
            },
            {
                "sturgia",
                new()
                {
                    "{=78HmDOmh}Tragalla",
                    "{=2gxRtC4M}Tihr",
                    "{=1QRisNZ8}Wercheg",
                    "{=mciKWCXz}Alburq",
                    "{=wquHRX1a}Chalbek",
                    "{=uOnd5NOi}Curin",
                    "{=eb1lTxcg}Hrus",
                    "{=Dvs5IlpE}Jelbegi",
                    "{=C0Lxqtaa}Knudarr",
                    "{=LwGSxemg}Tehlrog",
                    "{=QniO32gk}Aldelen",
                    "{=SPWVORXL}Ambean",
                    "{=ggpOsqyQ}Buillin",
                    "{=HVMKm9TL}Fearichen",
                    "{=vAsQx5yo}Fenada",
                    "{=XUmidhIJ}Gisim",
                    "{=IkMHxdfH}Haen",
                    "{=nGAYbe0Z}Jayek",
                    "{=i3QpFo60}Jelbegi",
                    "{=cckFsFjh}Kulum",
                    "{=0sU6mK84}Kwynn",
                    "{=5hFwgLgJ}Mechin",
                    "{=aMbbajXx}Odasan",
                    "{=SeEJ2zuw}Rizi",
                    "{=MLvw1Pxt}Ruvar",
                    "{=IrIFkrHZ}Udiniad",
                    "{=LHE0xjg1}Vayejeg",
                }
            },
            {
                "vlandia",
                new()
                {
                    "{=FD8sniLW}Echenad",
                    "{=8NkMfWHn}Raedia",
                    "{=piyHeKyM}Dhirim",
                    "{=8Y3wgpLi}Praven",
                    "{=yQlAJYuA}Suno",
                    "{=HUUOh35g}Uxkhal",
                    "{=e7h8mCpf}Derchios",
                    "{=V9wYAqHZ}Haringoth",
                    "{=5CVj9I06}Kelredan",
                    "{=uLOFze6g}Reindi",
                    "{=QQA4T9vg}Rindyar",
                    "{=xpLcu6bR}Ryibelet",
                    "{=I0FOtfwt}Senuzgda",
                    "{=BKyLwpjT}Tevarin",
                    "{=t1flBIrH}Tilbaut",
                    "{=udQOxzOj}Vyincourd",
                    "{=JNVa1azk}Amere",
                    "{=rFJC0mXH}Azgad",
                    "{=1zlQk8RB}Balanli",
                    "{=V5T0fzsG}Burglen",
                    "{=bVjU79Tn}Chide",
                    "{=dFxvU13H}Elberl",
                    "{=7kLsltIf}Ehlerdah",
                    "{=XJJ3nxkF}Emirin",
                    "{=9fgZRhiC}Gisim",
                    "{=WPnVGzk8}Ibiran",
                    "{=WKyofVcP}Iyindah",
                    "{=lz5Xr4F1}Nemeja",
                    "{=ZkXPCZUE}Nomar",
                    "{=9jAc6grT}Rduna",
                    "{=94L2s8cH}Ruluns",
                    "{=kX4NIQxU}Ryibelet",
                    "{=h2SaSr4e}Tadsamesh",
                    "{=SZLdfWI9}Tahlberl",
                    "{=GiWgcvib}Tosdhar",
                    "{=BzUpDzL0}Tshibtin",
                    "{=qDzxntWH}Ushkuru",
                    "{=6OPt8ox0}Veidar",
                    "{=nZqhwdTJ}Yalibe",
                    "{=8xTh5udH}Yaragar",
                }
            },
            {
                "empire",
                new()
                {
                    "{=LGqYPQQD}Lamonnos",
                    "{=KJPW3yb4}Nasus",
                    "{=OyLkibYo}Tylamahos",
                    "{=gNbGiI9p}Iasessos",
                    "{=ty1393cd}Zakrios",
                    "{=xJPv0Tzj}Samissos",
                    "{=AiHcQH4G}Thorosse",
                    "{=VuShnNy1}Olympale",
                    "{=WLGUwy9g}Diosconia",
                    "{=XQzaMWsl}Zanclaca",
                    "{=s5p2LcPn}Lissada",
                    "{=htjATdi1}Artylos",
                    "{=1cBpuRVl}Vasiri",
                    "{=YC1VI5Vs}Kamestias",
                    "{=6dTpjXIG}Erosthena",
                    "{=pzHthnN3}Kamaraza",
                    "{=ITwLbhNa}Pydnippia",
                    "{=qnRyyhx6}Abdocaea",
                    "{=cIGCqXYr}Aytanes",
                    "{=G375MfCN}Thermedon",
                    "{=I4LbIJpd}Byzamahos",
                    "{=6QtcERL6}Assapolis",
                    "{=eYehtDrU}Knossaphos",
                    "{=ax8cYBtr}Epidicyon",
                    "{=V3K7oVtB}Solastro",
                    "{=KYas1WAV}Piserna",
                    "{=opwrHv6z}Corconia",
                    "{=zPkDoxkU}Kerkos",
                    "{=mpnSLYUt}Assikon",
                    "{=PiblnoTV}Gournaphos",
                }
            },
        };

        public static TextObject GenerateKingdomName(RebelFaction rebelFaction)
        {
            string? kingdomTitle;
            var culture = rebelFaction.Clans.Where(x => !x.IsEliminated).Select(x => x.Culture.StringId).GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;
            CultureToKingdomTitles.TryGetValue(culture, out var cultureTitles);
            if (cultureTitles is not null && cultureTitles.Any() && MBRandom.RandomFloat < 0.5)
            {
                kingdomTitle = ResolveTitle(cultureTitles);
            }
            else
            {
                kingdomTitle = ResolveTitle(CommonKingdomTitles);
            }

            var kingdomName = CultureToKingdomNames.TryGetValue(culture, out var value) && value.Any() ? new TextObject(value.GetRandomElement()) : rebelFaction.SponsorClan.Name;

            return new TextObject(kingdomTitle, new Dictionary<string, object> { { "CLAN_NAME", kingdomName } });
        }

        private static string ResolveTitle(List<TitleSelection> selections)
        {
            string? kingdomTitle = null;

            if (selections == null || selections.Sum(x => x.Weight) == 0f)
                throw new MBException("Can't retrieve title from selections with no selections or zero weights.");

            while (kingdomTitle == null)
            {
                var tempKingdomTitle = selections.GetRandomElement();
                if (tempKingdomTitle.Weight > MBRandom.RandomFloat)
                    kingdomTitle = tempKingdomTitle.Name;
            }
            return kingdomTitle;
        }

        public static TextObject GenerateFactionName(Clan sponsorClan) => new TextObject(FactionNames.GetRandomElementInefficiently()).SetTextVariable("CLAN_NAME", sponsorClan.Name);

        private readonly struct TitleSelection
        {
            public TitleSelection(string name, float weight = 1f)
            {
                Name = name;
                Weight = weight;
            }

            public string Name { get; }
            public float Weight { get; }
        }
    }
}