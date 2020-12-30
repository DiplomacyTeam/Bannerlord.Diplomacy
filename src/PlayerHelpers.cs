﻿using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes
{
    class PlayerHelpers
    {
        public static Kingdom GetOpposingKingdomIfPlayerKingdomProvided(Kingdom kingdom1, Kingdom kingdom2)
        {
            if (kingdom1 == Hero.MainHero.MapFaction || kingdom2 == Hero.MainHero.MapFaction)
            {
                Kingdom opposingKingdom = kingdom1 == Hero.MainHero.MapFaction ? kingdom2 : kingdom1;
                return opposingKingdom;
            }
            return default;
        }
    }
}
