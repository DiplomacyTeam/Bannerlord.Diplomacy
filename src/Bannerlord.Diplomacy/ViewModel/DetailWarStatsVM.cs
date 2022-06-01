using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Diplomacy.ViewModel
{
    internal class DetailWarStatsVM : TaleWorlds.Library.ViewModel
    {
        [DataSourceProperty]
        public int TotalStrength { get; set; }
        [DataSourceProperty]
        public int Fiefs { get; set; }
        [DataSourceProperty]
        public string TotalStrengthLabel { get; set; }
        [DataSourceProperty]
        public string FiefsLabel { get; set; }

        public DetailWarStatsVM(Kingdom kingdom)
        {
            TotalStrength = Convert.ToInt32(Math.Round(kingdom.TotalStrength));
            Fiefs = kingdom.Fiefs.Count;
            TotalStrengthLabel = GameTexts.FindText("str_total_strength").ToString();
            FiefsLabel = GameTexts.FindText("str_fiefs").ToString();
        }
    }
}