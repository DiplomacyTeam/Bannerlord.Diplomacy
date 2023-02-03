using Bannerlord.ButterLib.Common.Helpers;

using Diplomacy.WarExhaustion.EventRecords;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Diplomacy.WarExhaustion
{
    internal sealed partial class WarExhaustionManager
    {
        private const string _TSummary = "{=i5VSEDE3}{START_DATE} - {END_DATE}: {?COUNT.PLURAL_FORM}Total of {COUNT} previous entries{?}{COUNT} previous entry{\\?}";

        public readonly struct WarExhaustionBreakdown
        {
            public WarExhaustionType Type { init; get; }
            public int ValueFaction1 { init; get; }
            public int ValueFaction2 { init; get; }
            public float WarExhaustionValueFaction1 { init; get; }
            public float WarExhaustionValueFaction2 { init; get; }
            public List<(TextObject EventDescription, float ExhaustionValue)>? EventRecordsFaction1 { init; get; }
            public List<(TextObject EventDescription, float ExhaustionValue)>? EventRecordsFaction2 { init; get; }
        }

        internal List<WarExhaustionBreakdown> GetWarExhaustionBreakdown(Kingdom kingdom1, Kingdom kingdom2)
        {
            var result = new List<WarExhaustionBreakdown>();
            var stance = kingdom1.GetStanceWith(kingdom2);

            var key = CreateKey(kingdom1, kingdom2, out var kingdoms);
            if (key is null || kingdoms is null)
                return result;

            if (_warExhaustionEventRecords.TryGetValue(key!, out var warExhaustionEventRecords))
            {
                GetCasualtiesBreakdown(result, warExhaustionEventRecords, kingdoms.ReversedKeyOrder);
                GetCaravanRaidsBreakdown(result, warExhaustionEventRecords, kingdoms.ReversedKeyOrder);
                GetRaidsBreakdown(result, warExhaustionEventRecords, kingdoms.ReversedKeyOrder);
                GetSiegesBreakdown(result, warExhaustionEventRecords, kingdoms.ReversedKeyOrder);
                GetHeroesImprisonedBreakdown(result, warExhaustionEventRecords, kingdoms.ReversedKeyOrder);
                GetHeroesPerishedBreakdown(result, warExhaustionEventRecords, kingdoms.ReversedKeyOrder);
                //Bottom
                GetOccupiedBreakdown(result, warExhaustionEventRecords, kingdoms.ReversedKeyOrder);
                GetDailyBreakdown(result, warExhaustionEventRecords, kingdoms.ReversedKeyOrder);
                GetDivineBreakdown(result, warExhaustionEventRecords, kingdoms.ReversedKeyOrder);
            }
            else
            {
                GetWarExhaustionBreakdownOldWay(kingdom1, kingdom2, result, stance, key, kingdoms);
            }

            return result;
        }
        private static void GetCasualtiesBreakdown(List<WarExhaustionBreakdown> result, List<WarExhaustionEventRecord> warExhaustionEventRecords, bool reversedKeyOrder) => GetBreakdownByEventType(WarExhaustionType.Casualty, result, warExhaustionEventRecords, reversedKeyOrder, useValue: true);

        private static void GetDailyBreakdown(List<WarExhaustionBreakdown> result, List<WarExhaustionEventRecord> warExhaustionEventRecords, bool reversedKeyOrder) => GetBreakdownByEventType(WarExhaustionType.Daily, result, warExhaustionEventRecords, reversedKeyOrder, useValue: true);

        private void GetCaravanRaidsBreakdown(List<WarExhaustionBreakdown> result, List<WarExhaustionEventRecord> warExhaustionEventRecords, bool reversedKeyOrder) => GetBreakdownByEventType(WarExhaustionType.CaravanRaid, result, warExhaustionEventRecords, reversedKeyOrder);

        private void GetRaidsBreakdown(List<WarExhaustionBreakdown> result, List<WarExhaustionEventRecord> warExhaustionEventRecords, bool reversedKeyOrder) => GetBreakdownByEventType(WarExhaustionType.Raid, result, warExhaustionEventRecords, reversedKeyOrder);

        private void GetSiegesBreakdown(List<WarExhaustionBreakdown> result, List<WarExhaustionEventRecord> warExhaustionEventRecords, bool reversedKeyOrder) => GetBreakdownByEventType(WarExhaustionType.Siege, result, warExhaustionEventRecords, reversedKeyOrder);

        private void GetHeroesImprisonedBreakdown(List<WarExhaustionBreakdown> result, List<WarExhaustionEventRecord> warExhaustionEventRecords, bool reversedKeyOrder) => GetBreakdownByEventType(WarExhaustionType.HeroImprisoned, result, warExhaustionEventRecords, reversedKeyOrder, addIfEmpty: false);

        private void GetHeroesPerishedBreakdown(List<WarExhaustionBreakdown> result, List<WarExhaustionEventRecord> warExhaustionEventRecords, bool reversedKeyOrder) => GetBreakdownByEventType(WarExhaustionType.HeroPerished, result, warExhaustionEventRecords, reversedKeyOrder, addIfEmpty: false);

        private void GetOccupiedBreakdown(List<WarExhaustionBreakdown> result, List<WarExhaustionEventRecord> warExhaustionEventRecords, bool reversedKeyOrder) => GetBreakdownByEventType(WarExhaustionType.Occupied, result, warExhaustionEventRecords, reversedKeyOrder, addIfEmpty: false);

        private void GetDivineBreakdown(List<WarExhaustionBreakdown> result, List<WarExhaustionEventRecord> warExhaustionEventRecords, bool reversedKeyOrder) => GetBreakdownByEventType(WarExhaustionType.Divine, result, warExhaustionEventRecords, reversedKeyOrder, addIfEmpty: false);

        private static void GetBreakdownByEventType(WarExhaustionType warExhaustionType, List<WarExhaustionBreakdown> result, List<WarExhaustionEventRecord> warExhaustionEventRecords, bool reversedKeyOrder, bool useValue = false, bool addIfEmpty = true)
        {
            var eventRecords = warExhaustionEventRecords.Where(r => r.WarExhaustionType == warExhaustionType).OrderBy(r => r.EventDate).ToList();
            if (!eventRecords.Any() && !addIfEmpty)
                return;

            int valueFaction1 = 0, valueFaction2 = 0;
            float exhaustionValueFaction1 = 0f, exhaustionValueFaction2 = 0f;
            List<(TextObject EventDescription, float ExhaustionValue, CampaignTime EventDate)> listFaction1 = new(), listFaction2 = new();
            if (reversedKeyOrder)
            {
                foreach (var eventRec in eventRecords)
                {
                    valueFaction1 += useValue ? eventRec.Faction2Value : eventRec.Faction2Effected ? 1 : 0;
                    valueFaction2 += useValue ? eventRec.Faction1Value : eventRec.Faction1Effected ? 1 : 0;
                    exhaustionValueFaction1 += eventRec.Faction2ExhaustionValue;
                    exhaustionValueFaction2 += eventRec.Faction1ExhaustionValue;
                    if (eventRec.Faction2Effected)
                        listFaction1.Add((eventRec.Faction2EventDescription!, eventRec.Faction2ExhaustionValue, eventRec.EventDate));
                    if (eventRec.Faction1Effected)
                        listFaction2.Add((eventRec.Faction1EventDescription!, eventRec.Faction1ExhaustionValue, eventRec.EventDate));
                }
            }
            else
            {
                foreach (var eventRec in eventRecords)
                {
                    valueFaction1 += useValue ? eventRec.Faction1Value : eventRec.Faction1Effected ? 1 : 0;
                    valueFaction2 += useValue ? eventRec.Faction2Value : eventRec.Faction2Effected ? 1 : 0;
                    exhaustionValueFaction1 += eventRec.Faction1ExhaustionValue;
                    exhaustionValueFaction2 += eventRec.Faction2ExhaustionValue;
                    if (eventRec.Faction1Effected)
                        listFaction1.Add((eventRec.Faction1EventDescription!, eventRec.Faction1ExhaustionValue, eventRec.EventDate));
                    if (eventRec.Faction2Effected)
                        listFaction2.Add((eventRec.Faction2EventDescription!, eventRec.Faction2ExhaustionValue, eventRec.EventDate));
                }
            }

            result.Add(new WarExhaustionBreakdown
            {
                Type = warExhaustionType,
                ValueFaction1 = valueFaction1,
                ValueFaction2 = valueFaction2,
                WarExhaustionValueFaction1 = exhaustionValueFaction1,
                WarExhaustionValueFaction2 = exhaustionValueFaction2,
                EventRecordsFaction1 = GetListOfLatestEntriesWithSummary(listFaction1),
                EventRecordsFaction2 = GetListOfLatestEntriesWithSummary(listFaction2),
            });
        }

        private static List<(TextObject EventDescription, float ExhaustionValue)> GetListOfLatestEntriesWithSummary(List<(TextObject EventDescription, float ExhaustionValue, CampaignTime EventDate)> originalList)
        {
            var recordsToShow = originalList.OrderByDescending(rec => rec.EventDate).Take(Settings.Instance!.MaxShownBreakdownEntries).OrderBy(rec => rec.EventDate).ToList();
            var recordsToSummarize = originalList.Except(recordsToShow).OrderBy(rec => rec.EventDate).ToList();

            if (!recordsToSummarize.Any())
                return recordsToShow.Select(x => (x.EventDescription, x.ExhaustionValue)).ToList();

            CampaignTime minDate = CampaignTime.Never, maxDate = CampaignTime.Zero; float totalValue = 0f; int recordCount = 0;
            foreach (var (_, exhaustionValue, eventDate) in recordsToSummarize)
            {
                totalValue += exhaustionValue;
                recordCount++;
                minDate = eventDate < minDate ? eventDate : minDate;
                maxDate = eventDate > maxDate ? eventDate : maxDate;
            }

            TextObject summaryTextObject = new(_TSummary, new() { ["START_DATE"] = minDate.ToString(), ["END_DATE"] = maxDate.ToString() });
            LocalizationHelper.SetNumericVariable(summaryTextObject, "COUNT", recordCount);
            List<(TextObject EventDescription, float ExhaustionValue)> result = new() { (summaryTextObject, totalValue) };
            result.AddRange(recordsToShow.Select(x => (x.EventDescription, x.ExhaustionValue)));

            return result;
        }

        private void GetWarExhaustionBreakdownOldWay(Kingdom kingdom1, Kingdom kingdom2, List<WarExhaustionBreakdown> result, StanceLink stance, string key, Kingdoms kingdoms)
        {
            if (!_warExhaustionRates.TryGetValue(key, out var multiplier))
            {
                RegisterWarExhaustionMultiplier(kingdoms);
                multiplier = _warExhaustionRates[key];
            }
            var faction1Multiplier = (kingdoms.ReversedKeyOrder ? multiplier.Faction2Value : multiplier.Faction1Value);
            var faction2Multiplier = (kingdoms.ReversedKeyOrder ? multiplier.Faction1Value : multiplier.Faction2Value);

            var valueFaction1 = stance.GetCasualties(kingdom1);
            var valueFaction2 = stance.GetCasualties(kingdom2);
            result.Add(new WarExhaustionBreakdown
            {
                Type = WarExhaustionType.Casualty,
                ValueFaction1 = valueFaction1,
                ValueFaction2 = valueFaction2,
                WarExhaustionValueFaction1 = valueFaction1 * Settings.Instance!.WarExhaustionPerCasualty * faction1Multiplier,
                WarExhaustionValueFaction2 = valueFaction2 * Settings.Instance!.WarExhaustionPerCasualty * faction2Multiplier
            });
            valueFaction1 = stance.GetSuccessfulRaids(kingdom2);
            valueFaction2 = stance.GetSuccessfulRaids(kingdom1);
            result.Add(new WarExhaustionBreakdown
            {
                Type = WarExhaustionType.Raid,
                ValueFaction1 = valueFaction1,
                ValueFaction2 = valueFaction2,
                WarExhaustionValueFaction1 = valueFaction1 * Settings.Instance!.WarExhaustionPerRaid * faction1Multiplier,
                WarExhaustionValueFaction2 = valueFaction2 * Settings.Instance!.WarExhaustionPerRaid * faction2Multiplier
            });
            valueFaction1 = stance.GetSuccessfulSieges(kingdom2);
            valueFaction2 = stance.GetSuccessfulSieges(kingdom1);
            result.Add(new WarExhaustionBreakdown
            {
                Type = WarExhaustionType.Siege,
                ValueFaction1 = valueFaction1,
                ValueFaction2 = valueFaction2,
                WarExhaustionValueFaction1 = valueFaction1 * Settings.Instance!.WarExhaustionPerSiege * faction1Multiplier,
                WarExhaustionValueFaction2 = valueFaction2 * Settings.Instance!.WarExhaustionPerSiege * faction2Multiplier
            });
            valueFaction1 = (int) stance.WarStartDate.ElapsedDaysUntilNow;
            result.Add(new WarExhaustionBreakdown
            {
                Type = WarExhaustionType.Daily,
                ValueFaction1 = valueFaction1,
                ValueFaction2 = valueFaction1,
                WarExhaustionValueFaction1 = valueFaction1 * Settings.Instance!.WarExhaustionPerDay,
                WarExhaustionValueFaction2 = valueFaction1 * Settings.Instance!.WarExhaustionPerDay
            });
        }
    }
}