using TaleWorlds.Library;
using TaleWorlds.Localization;

using static Diplomacy.WarExhaustionManager;

namespace Diplomacy.ViewModel
{
    internal class WarExhaustionBreakdownVM : TaleWorlds.Library.ViewModel
    {
        [DataSourceProperty] public string Text { get; set; }

        [DataSourceProperty] public int ValueFaction1 { get; set; }

        [DataSourceProperty] public int ValueFaction2 { get; set; }

        [DataSourceProperty] public string WarExhaustionValueFaction1 { get; set; }

        [DataSourceProperty] public string WarExhaustionValueFaction2 { get; set; }

        public WarExhaustionBreakdownVM(WarExhaustionBreakdown breakdown)
        {
            switch (breakdown.Type)
            {
                case WarExhaustionType.Casualty:
                    Text = new TextObject("{=zYVH0xYa}Casualties").ToString();
                    break;
                case WarExhaustionType.Raid:
                    Text = new TextObject("{=elRjo1ab}Villages Raided").ToString();
                    break;
                case WarExhaustionType.Siege:
                    Text = new TextObject("{=DrNBDhx3}Fiefs Lost").ToString();
                    break;
                case WarExhaustionType.Daily:
                    Text = new TextObject("{=XIPMI3gR}War Duration").ToString();
                    break;
                default:
                    Text = TextObject.Empty.ToString();
                    break;
            }

            ValueFaction1 = breakdown.ValueFaction1;
            ValueFaction2 = breakdown.ValueFaction2;
            WarExhaustionValueFaction1 = string.Format("{1}{0:F1}{2}", breakdown.WarExhaustionValueFaction1, "+", "%");
            WarExhaustionValueFaction2 = string.Format("{1}{0:F1}{2}", breakdown.WarExhaustionValueFaction2, "+", "%");
        }
    }
}