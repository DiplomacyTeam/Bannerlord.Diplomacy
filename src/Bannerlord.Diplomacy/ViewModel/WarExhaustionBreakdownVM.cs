using Diplomacy.Helpers;
using Diplomacy.WarExhaustion;

using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

using static Diplomacy.WarExhaustion.WarExhaustionManager;

namespace Diplomacy.ViewModel
{
    internal class WarExhaustionBreakdownVM : TaleWorlds.Library.ViewModel
    {
        [DataSourceProperty]
        public string Text { get; set; }

        [DataSourceProperty]
        public int ValueFaction1 { get; set; }

        [DataSourceProperty]
        public int ValueFaction2 { get; set; }

        [DataSourceProperty]
        public string WarExhaustionValueFaction1 { get; set; }

        [DataSourceProperty]
        public string WarExhaustionValueFaction2 { get; set; }

        [DataSourceProperty]
        public BasicTooltipViewModel? Faction1ValueHint { get; set; }

        [DataSourceProperty]
        public BasicTooltipViewModel? Faction2ValueHint { get; set; }

        public WarExhaustionBreakdownVM(WarExhaustionBreakdown breakdown)
        {
            Text = breakdown.Type switch
            {
                WarExhaustionType.Casualty => new TextObject("{=zYVH0xYa}Casualties").ToString(),
                WarExhaustionType.CaravanRaid => new TextObject("{=y7XY860U}Caravans Raided").ToString(),
                WarExhaustionType.Raid => new TextObject("{=elRjo1ab}Villages Raided").ToString(),
                WarExhaustionType.Siege => new TextObject("{=DrNBDhx3}Fiefs Lost").ToString(),
                WarExhaustionType.HeroImprisoned => new TextObject("{=X1MIWYzy}Heroes Imprisoned").ToString(),
                WarExhaustionType.HeroPerished => new TextObject("{=6FLtWxSp}Heroes Perished").ToString(),
                WarExhaustionType.Occupied => new TextObject("{=sCRuliNO}Fully Occupied").ToString(),
                WarExhaustionType.Daily => new TextObject("{=XIPMI3gR}War Duration").ToString(),
                WarExhaustionType.Divine => new TextObject("{=FVyFJShW}Divine Interventions").ToString(),
                _ => TextObject.Empty.ToString(),
            };
            ValueFaction1 = breakdown.ValueFaction1;
            ValueFaction2 = breakdown.ValueFaction2;
            WarExhaustionValueFaction1 = string.Format("{1}{0:F1}{2}", breakdown.WarExhaustionValueFaction1, breakdown.WarExhaustionValueFaction1 >= 0f ? "+" : "", "%");
            WarExhaustionValueFaction2 = string.Format("{1}{0:F1}{2}", breakdown.WarExhaustionValueFaction2, breakdown.WarExhaustionValueFaction2 >= 0f ? "+" : "", "%");
            Faction1ValueHint = UpdateValueHint(1, breakdown);
            Faction2ValueHint = UpdateValueHint(2, breakdown);
        }

        private BasicTooltipViewModel UpdateValueHint(int factionIndex, WarExhaustionBreakdown breakdown)
        {
            var (factionValue, factionExhaustionValue, factionEventRecords) = factionIndex switch
            {
                1 => (ValueFaction1, WarExhaustionValueFaction1, breakdown.EventRecordsFaction1),
                2 => (ValueFaction2, WarExhaustionValueFaction2, breakdown.EventRecordsFaction2),
                _ => throw new ArgumentOutOfRangeException(nameof(factionIndex)),
            };

            var list = new List<TooltipProperty>();
            if (factionEventRecords is not null && factionEventRecords.Any())
            {
                list.Add(new TooltipProperty(Text, factionValue > 0 ? factionValue.ToString() : string.Empty, 0, false, TooltipProperty.TooltipPropertyFlags.Title));
                foreach (var (eventDescription, exhaustionValue) in factionEventRecords)
                    list.Add(new TooltipProperty(eventDescription.ToString(), $"{StringHelper.GetPlusPrefixed(exhaustionValue)}%", 0));

                if (factionEventRecords.Count > 1)
                {
                    list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
                    list.Add(new TooltipProperty(new TextObject("{=1kGleqyk}Resulting war exhaustion").ToString(), factionExhaustionValue, 0, false, TooltipProperty.TooltipPropertyFlags.RundownResult));
                }
            }

            return new BasicTooltipViewModel(() => list);
        }
    }
}