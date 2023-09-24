using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;

using Diplomacy.Events;
using Diplomacy.Extensions;

using HarmonyLib.BUTR.Extensions;

using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace Diplomacy.CivilWar.Actions
{
    public class ChangeKingdomBannerAction
    {
        private delegate uint GetPrimaryBannerColorDelegate(Kingdom instance);
        private delegate void SetPrimaryBannerColorDelegate(Kingdom instance, uint value);

        private delegate uint GetSecondaryBannerColorDelegate(Kingdom instance);
        private delegate void SetSecondaryBannerColorDelegate(Kingdom instance, uint value);

        private delegate void SetColorDelegate(Kingdom instance, uint value);

        private delegate void SetColor2Delegate(Kingdom instance, uint value);

        private delegate void UpdateBannerColorsAccordingToKingdomDelegate(Clan instance);

        private static readonly GetPrimaryBannerColorDelegate? GetPrimaryBannerColor =
            AccessTools2.GetPropertyGetterDelegate<GetPrimaryBannerColorDelegate>(typeof(Kingdom), "PrimaryBannerColor");
        private static readonly SetPrimaryBannerColorDelegate? SetPrimaryBannerColor =
            AccessTools2.GetPropertySetterDelegate<SetPrimaryBannerColorDelegate>(typeof(Kingdom), "PrimaryBannerColor");

        private static readonly GetSecondaryBannerColorDelegate? GetSecondaryBannerColor =
            AccessTools2.GetPropertyGetterDelegate<GetSecondaryBannerColorDelegate>(typeof(Kingdom), "SecondaryBannerColor");
        private static readonly SetSecondaryBannerColorDelegate? SetSecondaryBannerColor =
            AccessTools2.GetPropertySetterDelegate<SetSecondaryBannerColorDelegate>(typeof(Kingdom), "SecondaryBannerColor");

        private static readonly SetColorDelegate? SetPrimaryColor =
            AccessTools2.GetPropertySetterDelegate<SetColorDelegate>(typeof(Kingdom), "Color");
        private static readonly SetColor2Delegate? SetSecondaryColor =
            AccessTools2.GetPropertySetterDelegate<SetColor2Delegate>(typeof(Kingdom), "Color2");

        private static readonly UpdateBannerColorsAccordingToKingdomDelegate? UpdateBannerColorsAccordingToKingdom =
            AccessTools2.GetDelegate<UpdateBannerColorsAccordingToKingdomDelegate>(typeof(Clan), "UpdateBannerColorsAccordingToKingdom");

        public static void Apply(Kingdom kingdom, uint backgroundColor, uint sigilColor)
        {
            SetPrimaryBannerColor?.Invoke(kingdom, backgroundColor);
            SetSecondaryBannerColor?.Invoke(kingdom, sigilColor);
            SetPrimaryColor?.Invoke(kingdom, backgroundColor);
            SetSecondaryColor?.Invoke(kingdom, sigilColor);

            foreach (var clan in kingdom.Clans)
            {
                UpdateBannerColorsAccordingToKingdom?.Invoke(clan);
            }

            foreach (var mobileParty in MobileParty.All)
            {
                var owner = mobileParty.Party?.Owner;

                var clanKingdom = owner?.Clan?.Kingdom;

                if (clanKingdom == kingdom)
                {
#if v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115 || v116
                    var visuals = mobileParty.Party!.Visuals;
                    if (visuals != null)
                    {
                        visuals.SetMapIconAsDirty();
                    }
#else
                    mobileParty.Party!.SetVisualAsDirty();
#endif
                }
            }

            foreach (var settlement in kingdom.Settlements)
            {
#if v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115 || v116
                var visuals = settlement.Party.Visuals;
                if (visuals != null)
                {
                    visuals.SetMapIconAsDirty();
                }
#else
                settlement.Party?.SetVisualAsDirty();
#endif

                if (settlement.IsVillage && settlement.Village.VillagerPartyComponent != null)
                {
                    var party = settlement.Village.VillagerPartyComponent.MobileParty.Party;
                    if (party != null)
                    {
#if v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115 || v116
                        party.Visuals.SetMapIconAsDirty();
#else
                        party.SetVisualAsDirty();
#endif
                    }
                }
                else if ((settlement.IsCastle || settlement.IsTown) && settlement.Town.GarrisonParty != null)
                {
                    var party = settlement.Town.GarrisonParty.Party;
                    if (party != null)
                    {
#if v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115 || v116
                        party.Visuals.SetMapIconAsDirty();
#else
                        party.SetVisualAsDirty();
#endif
                    }
                }
            }

            DiplomacyEvents.Instance.OnKingdomBannerChanged(kingdom);
        }

        public static void Apply(Kingdom kingdom, bool isRebelKingdom = false)
        {
            uint backgroundColor;
            uint sigilColor;

            if (isRebelKingdom)
            {
                backgroundColor = RebelBackgroundColor;
                sigilColor = GetUniqueSigilColor(backgroundColor);
            }
            else
            {
                // choose random unused color from the palette
                var currentBackgroundColors = KingdomExtensions.AllActiveKingdoms.Where(x => !x.IsEliminated).Select(x => GetPrimaryBannerColor?.Invoke(x)).ToList();
                backgroundColor = BannerManager.ColorPalette.Where(x => !currentBackgroundColors.Contains(x.Value.Color)).GetRandomElementInefficiently().Value.Color;
                sigilColor = GetUniqueSigilColor(backgroundColor);
            }

            Apply(kingdom, backgroundColor, sigilColor);
        }

        private static uint GetUniqueSigilColor(uint backgroundColor)
        {
            var background = GetRgb(backgroundColor);

            var selectedColor = BannerManager.ColorPalette.Where(x => background.Compare(GetRgb(x.Value.Color), new Cie1976Comparison()) > 40).GetRandomElementInefficiently().Value.Color;
            if (backgroundColor == RebelBackgroundColor)
            {
                var currentSigilColors = KingdomExtensions.AllActiveKingdoms.Where(x => !x.IsEliminated && x.IsRebelKingdom()).Select(x => GetSecondaryBannerColor?.Invoke(x)).ToList();
                var colors = BannerManager.ColorPalette.Select(x => x.Value.Color)
                    .Where(x => background.Compare(GetRgb(x), new Cie1976Comparison()) > 40)
                    .Where(x => !currentSigilColors.Contains(x))
                    .ToList();

                if (colors.Any())
                    selectedColor = colors.GetRandomElementInefficiently();
            }

            return selectedColor;
        }

        private static uint RebelBackgroundColor => BannerManager.GetColor(99);

        private static Rgb GetRgb(uint color)
        {
            var hex = color.ToHexadecimalString();
            hex = hex.Substring(2);
            var rgb = new Hex(hex).To<Rgb>();
            return rgb;
        }
    }
}