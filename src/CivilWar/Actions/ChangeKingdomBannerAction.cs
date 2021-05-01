using Diplomacy.Event;
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
                backgroundColor = BannerManager.GetColor(99);
                sigilColor = BannerManager.ColorPalette.Where(x => x.Value.PlayerCanChooseForSigil).GetRandomElementInefficiently().Value.Color;
            }
            else
            {
                // choose random unused color from the palette
                List<uint> currentBackgroundColors = Kingdom.All.Where(x => !x.IsEliminated).Select(x => (uint)_primaryColorProp.GetValue(x)).ToList();
                backgroundColor = BannerManager.ColorPalette.Where(x => !currentBackgroundColors.Contains(x.Value.Color)).GetRandomElementInefficiently().Value.Color;
                sigilColor = BannerManager.ColorPalette.Where(x => x.Value.PlayerCanChooseForSigil).GetRandomElementInefficiently().Value.Color;
            }

            Apply(kingdom, backgroundColor, sigilColor);
        }
    }
}
