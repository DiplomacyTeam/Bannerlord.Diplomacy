using Diplomacy.Event;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace Diplomacy
{
    internal sealed class WarExhaustionManager
    {
        // new war exhaustion dictionary using Id
        [SaveableField(1)] private Dictionary<string, float> _warExhaustionById;

        [SaveableField(2)] private Dictionary<string, float> _warExhaustionMultiplier;

        public static WarExhaustionManager Instance { get; private set; } = default!;

        internal static float MaxWarExhaustion => 100f;

        internal static float MinWarExhaustion => 0f;

        internal WarExhaustionManager()
        {
            _warExhaustionById = new Dictionary<string, float>();
            _warExhaustionMultiplier = new Dictionary<string, float>();
            Instance = this;
        }

        public float GetWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            var key = CreateKey(kingdom1, kingdom2);
            return key is not null && _warExhaustionById.TryGetValue(key, out var warExhaustion)
                ? warExhaustion
                : 0f;
        }

        public float GetWarExhaustionRate(Kingdom kingdom1, Kingdom kingdom2)
        {
            var key = CreateKey(kingdom1, kingdom2);
            return key is not null && _warExhaustionMultiplier.TryGetValue(key, out var rate)
                ? rate
                : 1f;
        }

        public void ClearWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            var key = CreateKey(kingdom1, kingdom2);
            if (key is not null) _warExhaustionById[key] = 0;
        }

        internal void RegisterWarExhaustionMultiplier(Kingdom kingdom1, Kingdom kingdom2)
        {
            var average = (kingdom1.TotalStrength + kingdom2.TotalStrength) / 2;
            var multiplier = MBMath.ClampFloat(1000f / average, 0.25f, 1.0f);

            var key = CreateKey(kingdom1, kingdom2);
            var key2 = CreateKey(kingdom2, kingdom1);
            if (key is not null && key2 is not null)
            {
                _warExhaustionMultiplier![key!] = multiplier;
                _warExhaustionMultiplier![key2!] = multiplier;
            }
        }

        private static bool KingdomsAreValid(Kingdom? kingdom1, Kingdom? kingdom2)
        {
            return kingdom1 is not null && kingdom2 is not null && kingdom1.Id != default && kingdom2.Id != default;
        }

        private string? CreateKey(Kingdom kingdom1, Kingdom kingdom2)
        {
            return CreateKey(new Tuple<Kingdom, Kingdom>(kingdom1, kingdom2));
        }

        private string? CreateKey(Tuple<Kingdom, Kingdom> kingdoms)
        {
            return KingdomsAreValid(kingdoms.Item1, kingdoms.Item2)
                ? string.Join("+", kingdoms.Item1.Id, kingdoms.Item2.Id)
                : null;
        }

        private void AddDailyWarExhaustion(Tuple<Kingdom, Kingdom> kingdoms)
        {
            var warExhaustionToAdd = GetDailyWarExhaustionDelta();
            AddWarExhaustion(kingdoms.Item1, kingdoms.Item2, warExhaustionToAdd, WarExhaustionType.Daily);
        }

        public void AddCasualtyWarExhaustion(Kingdom kingdom1, Kingdom kingdom2, int casualties)
        {
            var warExhaustionToAdd = Settings.Instance!.WarExhaustionPerCasualty * casualties;
            AddWarExhaustion(kingdom1, kingdom2, warExhaustionToAdd, WarExhaustionType.Casualty);
        }

        public void AddSiegeWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            var warExhaustionToAdd = Settings.Instance!.WarExhaustionPerSiege;
            AddWarExhaustion(kingdom1, kingdom2, warExhaustionToAdd, WarExhaustionType.Siege);
        }

        public void AddOccupiedWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            AddWarExhaustion(kingdom1, kingdom2, float.MaxValue, WarExhaustionType.Occupied);
        }

        public void AddRaidWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            var warExhaustionToAdd = Settings.Instance!.WarExhaustionPerRaid;
            AddWarExhaustion(kingdom1, kingdom2, warExhaustionToAdd, WarExhaustionType.Raid);
        }

        private void AddWarExhaustion(Kingdom kingdom1, Kingdom kingdom2, float warExhaustionToAdd, WarExhaustionType warExhaustionType)
        {
            var key = CreateKey(kingdom1, kingdom2);
            if (key is null)
                return;
            float finalWarExhaustionDelta;
            if (warExhaustionType == WarExhaustionType.Daily)
            {
                finalWarExhaustionDelta = warExhaustionToAdd;
            }
            else
            {
                if (!_warExhaustionMultiplier!.TryGetValue(key, out var warExhaustionFactor))
                {
                    RegisterWarExhaustionMultiplier(kingdom1, kingdom2);
                    warExhaustionFactor = _warExhaustionMultiplier[key];
                }

                finalWarExhaustionDelta = warExhaustionToAdd * warExhaustionFactor;
            }

            if (_warExhaustionById.TryGetValue(key, out var currentValue))
                _warExhaustionById[key] = MBMath.ClampFloat(currentValue + finalWarExhaustionDelta, MinWarExhaustion, MaxWarExhaustion);
            else
                _warExhaustionById[key] = MBMath.ClampFloat(finalWarExhaustionDelta, MinWarExhaustion, MaxWarExhaustion);

            if (Settings.Instance!.EnableWarExhaustionDebugMessages && kingdom1 == Hero.MainHero.MapFaction)
            {
                var information =
                    $"Added {finalWarExhaustionDelta} {Enum.GetName(typeof(WarExhaustionType), warExhaustionType)} war exhaustion to {kingdom1.Name}'s war with {kingdom2.Name}";
                InformationManager.DisplayMessage(new InformationMessage(information, Color.FromUint(4282569842U)));
            }

            Events.Instance.OnWarExhaustionAdded(new WarExhaustionEvent(kingdom1, kingdom2, warExhaustionType, warExhaustionToAdd));
        }

        private float GetDailyWarExhaustionDelta()
        {
            return Settings.Instance!.WarExhaustionPerDay;
        }

        public void UpdateDailyWarExhaustionForAllKingdoms()
        {
            foreach (var kingdom in Kingdom.All)
            {
                var enemyKingdoms = FactionManager.GetEnemyKingdoms(kingdom);
                foreach (var enemyKingdom in enemyKingdoms)
                {
                    var warStartDate = kingdom.GetStanceWith(enemyKingdom).WarStartDate.ElapsedDaysUntilNow;
                    if (warStartDate >= 1.0f) AddDailyWarExhaustion(Tuple.Create(kingdom, enemyKingdom));
                }
            }
        }

        public bool HasMaxWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            return GetWarExhaustion(kingdom1, kingdom2) >= MaxWarExhaustion;
        }

        internal List<WarExhaustionBreakdown> GetWarExhaustionBreakdown(Kingdom kingdom1, Kingdom kingdom2)
        {
            var result = new List<WarExhaustionBreakdown>();
            var stance = kingdom1.GetStanceWith(kingdom2);

            var key = CreateKey(kingdom1, kingdom2);
            if (!_warExhaustionMultiplier.TryGetValue(key!, out var multiplier))
            {
                RegisterWarExhaustionMultiplier(kingdom1, kingdom2);
                multiplier = _warExhaustionMultiplier[key!];
            }

            var valueFaction1 = stance.GetCasualties(kingdom1);
            var valueFaction2 = stance.GetCasualties(kingdom2);
            result.Add(new WarExhaustionBreakdown()
            {
                Type = WarExhaustionType.Casualty,
                ValueFaction1 = valueFaction1,
                ValueFaction2 = valueFaction2,
                WarExhaustionValueFaction1 = valueFaction1 * Settings.Instance!.WarExhaustionPerCasualty * multiplier,
                WarExhaustionValueFaction2 = valueFaction2 * Settings.Instance!.WarExhaustionPerCasualty * multiplier
            });
            valueFaction1 = stance.GetSuccessfulRaids(kingdom2);
            valueFaction2 = stance.GetSuccessfulRaids(kingdom1);
            result.Add(new WarExhaustionBreakdown()
            {
                Type = WarExhaustionType.Raid,
                ValueFaction1 = valueFaction1,
                ValueFaction2 = valueFaction2,
                WarExhaustionValueFaction1 = valueFaction1 * Settings.Instance!.WarExhaustionPerRaid * multiplier,
                WarExhaustionValueFaction2 = valueFaction2 * Settings.Instance!.WarExhaustionPerRaid * multiplier
            });
            valueFaction1 = stance.GetSuccessfulSieges(kingdom2);
            valueFaction2 = stance.GetSuccessfulSieges(kingdom1);
            result.Add(new WarExhaustionBreakdown()
            {
                Type = WarExhaustionType.Siege,
                ValueFaction1 = valueFaction1,
                ValueFaction2 = valueFaction2,
                WarExhaustionValueFaction1 = valueFaction1 * Settings.Instance!.WarExhaustionPerSiege * multiplier,
                WarExhaustionValueFaction2 = valueFaction2 * Settings.Instance!.WarExhaustionPerSiege * multiplier
            });
            valueFaction1 = (int) stance.WarStartDate.ElapsedDaysUntilNow;
            result.Add(new WarExhaustionBreakdown()
            {
                Type = WarExhaustionType.Daily,
                ValueFaction1 = valueFaction1,
                ValueFaction2 = valueFaction2,
                WarExhaustionValueFaction1 = valueFaction1 * Settings.Instance!.WarExhaustionPerDay,
                WarExhaustionValueFaction2 = valueFaction1 * Settings.Instance!.WarExhaustionPerDay
            });

            return result;
        }

        public struct WarExhaustionBreakdown
        {
            public WarExhaustionType Type { init; get; }
            public int ValueFaction1 { init; get; }
            public int ValueFaction2 { init; get; }
            public float WarExhaustionValueFaction1 { init; get; }
            public float WarExhaustionValueFaction2 { init; get; }
        }

        public void Sync()
        {
            Instance = this;
            // ReSharper disable once ConstantNullCoalescingCondition
            _warExhaustionById ??= new Dictionary<string, float>();
            // ReSharper disable once ConstantNullCoalescingCondition
            _warExhaustionMultiplier ??= new Dictionary<string, float>();
        }

        internal enum WarExhaustionType
        {
            Casualty,
            Raid,
            Siege,
            Daily,
            Occupied
        }
    }
}