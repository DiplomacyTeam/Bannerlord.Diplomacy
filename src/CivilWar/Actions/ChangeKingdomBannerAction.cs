using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using Diplomacy.Event;
using Diplomacy.Extensions;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Diplomacy.CivilWar
{
    public class ChangeKingdomBannerAction
    {
        private static PropertyInfo _primaryColorProp = AccessTools.Property(typeof(Kingdom), "PrimaryBannerColor");
        private static PropertyInfo _secondaryColorProp = AccessTools.Property(typeof(Kingdom), "SecondaryBannerColor");
        private static MethodInfo _updateBannerColorsAccordingToKingdom = AccessTools.Method(typeof(Clan), "UpdateBannerColorsAccordingToKingdom");

        public static void Apply(Kingdom kingdom, uint backgroundColor, uint sigilColor)
        {
            _primaryColorProp.SetValue(kingdom, backgroundColor);
            _secondaryColorProp.SetValue(kingdom, sigilColor);

            foreach (Clan clan in kingdom.Clans)
            {
                _updateBannerColorsAccordingToKingdom.Invoke(clan, null);
            }

            foreach (MobileParty mobileParty in MobileParty.All)
            {
                Hero? owner = mobileParty.Party?.Owner;

                Kingdom? clanKingdom = owner?.Clan?.Kingdom;

                if (clanKingdom == kingdom)
                {
                    IPartyVisual visuals = mobileParty.Party!.Visuals;
                    if (visuals != null)
                    {
                        visuals.SetMapIconAsDirty();
                    }
                }
            }

            foreach (Settlement settlement in kingdom.Settlements)
            {
                IPartyVisual visuals = settlement.Party.Visuals;
                if (visuals != null)
                {
                    visuals.SetMapIconAsDirty();
                }

                if (settlement.IsVillage && settlement.Village.VillagerPartyComponent != null)
                {
                    var party = settlement.Village.VillagerPartyComponent.MobileParty.Party;
                    if (party != null)
                    {
                        party.Visuals.SetMapIconAsDirty();
                    }
                }
                else if ((settlement.IsCastle || settlement.IsTown) && settlement.Town.GarrisonParty != null)
                {
                    var party = settlement.Town.GarrisonParty.Party;
                    if (party != null)
                    {
                        party.Visuals.SetMapIconAsDirty();
                    }
                }
            }

            Events.Instance.OnKingdomBannerChanged(kingdom);
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
                List<uint> currentBackgroundColors = Kingdom.All.Where(x => !x.IsEliminated).Select(x => (uint)_primaryColorProp.GetValue(x)).ToList();
                backgroundColor = BannerManager.ColorPalette.Where(x => !currentBackgroundColors.Contains(x.Value.Color)).GetRandomElementInefficiently().Value.Color;
                sigilColor = GetUniqueSigilColor(backgroundColor);
            }

            Apply(kingdom, backgroundColor, sigilColor);
        }

        private static uint GetUniqueSigilColor(uint backgroundColor)
        {

            Rgb background = GetRgb(backgroundColor);

            uint selectedColor = BannerManager.ColorPalette.Where(x => background.Compare(GetRgb(x.Value.Color), new Cie1976Comparison()) > 40).GetRandomElementInefficiently().Value.Color;
            if (backgroundColor == RebelBackgroundColor)
            {
                List<uint> currentSigilColors = Kingdom.All.Where(x => !x.IsEliminated && x.IsRebelKingdom()).Select(x => (uint)_secondaryColorProp.GetValue(x)).ToList();
                var colors = BannerManager.ColorPalette.Select(x => x.Value.Color)
                    .Where(x => background.Compare(GetRgb(x), new Cie1976Comparison()) > 40)
                    .Where(x => !currentSigilColors.Contains(x));

                if (colors.Any())
                    selectedColor = colors.GetRandomElementInefficiently();
            }

            return selectedColor;
        }

        private static uint RebelBackgroundColor => BannerManager.GetColor(99);

        private static Rgb GetRgb(uint color)
        {
            string hex = color.ToHexadecimalString();
            hex = hex.Substring(2);
            var rgb = new Hex(hex).To<Rgb>();
            return rgb;
        }
    }
}
