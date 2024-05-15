using Diplomacy.Events;
using Diplomacy.Helpers;
using Diplomacy.WarExhaustion.EventRecords;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

using static Diplomacy.WarExhaustion.WarExhaustionRecord;

namespace Diplomacy.WarExhaustion
{
    internal sealed partial class WarExhaustionManager
    {
        internal record Kingdoms
        {
            public Kingdom Kingdom1 { get; }
            public Kingdom Kingdom2 { get; }
            public string? Key { get; } //Always [Faction with lesser ID]+[Faction with greater ID]
            public bool ReversedKeyOrder { get; }

            public Kingdoms(Kingdom kingdom1, Kingdom kingdom2) => (Kingdom1, Kingdom2, Key, ReversedKeyOrder) = (kingdom1, kingdom2, CreateKey(kingdom1, kingdom2, out var invertedOrder), invertedOrder);
            private static string? CreateKey(Kingdom kingdom1, Kingdom kingdom2, out bool invertedOrder) => (invertedOrder = kingdom1.Id > kingdom2.Id) ? string.Join("+", kingdom2.Id, kingdom1.Id) : string.Join("+", kingdom1.Id, kingdom2.Id);
            public bool Contains(IFaction? faction) => faction is Kingdom kingdom && (kingdom == Kingdom1 || kingdom == Kingdom2);
        }

        [SaveableField(1)]
        private Dictionary<string, WarExhaustionRecord> _warExhaustionScores;

        [SaveableField(2)]
        private Dictionary<string, WarExhaustionRecord> _warExhaustionRates;

        [SaveableField(3)]
        private Dictionary<string, List<WarExhaustionEventRecord>> _warExhaustionEventRecords;

        public static WarExhaustionManager? Instance { get; private set; } = default;

        internal const float MaxWarExhaustion = 100f;
        internal const float MinWarExhaustion = 0f;
        internal const float CriticalThresholdWarExhaustion = 0.75f;

        private const float BaseKingdomStrengthForExhaustionRate = 4000f;
        private const float MinExhaustionRate = 0.25f;
        private const float MaxExhaustionRate = 1.0f;

        internal WarExhaustionManager()
        {
            _warExhaustionScores = new();
            _warExhaustionRates = new();
            _warExhaustionEventRecords = new();
            Instance = this;
        }

        public float GetWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            var key = CreateKey(kingdom1, kingdom2, out var kingdoms);
            return key is not null && _warExhaustionScores.TryGetValue(key, out var warExhaustionRec)
                ? (kingdoms!.ReversedKeyOrder ? warExhaustionRec.Faction2Value : warExhaustionRec.Faction1Value)
                : 0f;
        }

        public float GetWarExhaustionRate(Kingdom kingdom1, Kingdom kingdom2)
        {
            var key = CreateKey(kingdom1, kingdom2, out var kingdoms);
            return key is not null && _warExhaustionRates.TryGetValue(key, out var warExhaustionRec)
                ? (kingdoms!.ReversedKeyOrder ? warExhaustionRec.Faction2Value : warExhaustionRec.Faction1Value)
                : 1f;
        }

        private WarExhaustionRecord GetWarExhaustionRates(Kingdoms kingdoms)
        {
            if (!_warExhaustionRates.TryGetValue(kingdoms.Key!, out var warExhaustionRate))
            {
                RegisterWarExhaustionMultiplier(kingdoms);
                warExhaustionRate = _warExhaustionRates[kingdoms.Key!];
            }
            return warExhaustionRate;
        }

        public ActiveQuestState GetWarExhaustionQuestState(Kingdom kingdom1, Kingdom kingdom2)
        {
            var key = CreateKey(kingdom1, kingdom2, out _);
            return key is not null && _warExhaustionScores.TryGetValue(key, out var warExhaustionRec) ? warExhaustionRec.QuestState : ActiveQuestState.None;
        }

        public void ClearWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            var key = CreateKey(kingdom1, kingdom2, out _, checkStates: false);
            if (key is not null)
            {
                _warExhaustionScores.Remove(key);
                _warExhaustionRates.Remove(key);
                _warExhaustionEventRecords.Remove(key);
                DiplomacyEvents.Instance.OnWarExhaustionInitialized(new WarExhaustionInitializedEvent(kingdom1, kingdom2));
            }
        }

        internal void RegisterWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            var key = CreateKey(kingdom1, kingdom2, out var kingdoms, checkStates: false);
            if (key is not null)
            {
                _warExhaustionScores[key] = new(0f, 0f, hasActiveQuest: !IsValidQuestState(kingdom1, kingdom2));
                RegisterWarExhaustionMultiplier(kingdoms!);
                _warExhaustionEventRecords[key] = new();
                DiplomacyEvents.Instance.OnWarExhaustionInitialized(new WarExhaustionInitializedEvent(kingdom1, kingdom2));
            }
        }

        internal void RegisterWarExhaustionMultiplier(Kingdoms kingdoms)
        {
            var key = kingdoms.Key;
            if (key is not null)
            {
                CalculateWarExhaustionMultiplier(kingdoms, out var multiplier1, out var multiplier2);
                _warExhaustionRates[key] = new(multiplier1, multiplier2, considerRangeLimits: false);
            }
        }

        private static void CalculateWarExhaustionMultiplier(Kingdoms kingdoms, out float multiplier1, out float multiplier2)
        {
            if (Settings.Instance!.IndividualWarExhaustionRates)
            {
                var kingdom1multiplier = MBMath.ClampFloat(CalculateMultiplier(kingdoms.Kingdom1.TotalStrength), MinExhaustionRate, MaxExhaustionRate);
                var kingdom2multiplier = MBMath.ClampFloat(CalculateMultiplier(kingdoms.Kingdom2.TotalStrength), MinExhaustionRate, MaxExhaustionRate);
                if (kingdoms.ReversedKeyOrder)
                {
                    multiplier1 = kingdom2multiplier;
                    multiplier2 = kingdom1multiplier;
                }
                else
                {
                    multiplier1 = kingdom1multiplier;
                    multiplier2 = kingdom2multiplier;
                }
            }
            else
            {
                var average = (kingdoms.Kingdom1.TotalStrength + kingdoms.Kingdom2.TotalStrength) / 2;
                multiplier1 = MBMath.ClampFloat(CalculateMultiplier(average), MinExhaustionRate, MaxExhaustionRate);
                multiplier2 = multiplier1;
            }

            static float CalculateMultiplier(float strength) => 1 - 0.25f * (Math.Max(strength / BaseKingdomStrengthForExhaustionRate, 1) - 1);
        }

        public List<WarExhaustionEventRecord> GetWarExhaustionEventRecords(Kingdom kingdom1, Kingdom kingdom2, out Kingdoms? kingdoms)
        {
            var key = CreateKey(kingdom1, kingdom2, out kingdoms);
            return key is not null && _warExhaustionEventRecords.TryGetValue(key, out var warExhaustionEventRecords)
                ? warExhaustionEventRecords
                : new();
        }

        public bool HasCriticalWarExhaustion(Kingdom kingdom1, Kingdom kingdom2, bool checkMaxWarExhaustion = false) => IsCriticalWarExhaustion(GetWarExhaustion(kingdom1, kingdom2), checkMaxWarExhaustion);

        public static bool IsCriticalWarExhaustion(float warExhaustionValue, bool checkMaxWarExhaustion = false) => (warExhaustionValue / MaxWarExhaustion >= CriticalThresholdWarExhaustion) && (!checkMaxWarExhaustion || warExhaustionValue < MaxWarExhaustion);

        private static bool KingdomsAreValid(Kingdom? kingdom1, Kingdom? kingdom2, bool checkStates) =>
            kingdom1 is not null && kingdom2 is not null
            && kingdom1.Id != default && kingdom2.Id != default && kingdom1.Id != kingdom2.Id
            && (!checkStates || (!kingdom1.IsEliminated && !kingdom2.IsEliminated && kingdom1.IsAtWarWith(kingdom2)));

        internal static string? CreateKey(Kingdom kingdom1, Kingdom kingdom2, out Kingdoms? kingdoms, bool checkStates = true)
        {
            if (!KingdomsAreValid(kingdom1, kingdom2, checkStates))
            {
                kingdoms = null;
                return null;
            }
            kingdoms = new(kingdom1, kingdom2);
            return kingdoms.Key;
        }

        public bool HasMaxWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            return GetWarExhaustion(kingdom1, kingdom2) >= MaxWarExhaustion;
        }

        public WarResult GetWarResult(Kingdom kingdom1, Kingdom kingdom2)
        {
            var key = CreateKey(kingdom1, kingdom2, out var kingdoms);
            if (key is null || kingdoms is null || !_warExhaustionScores.TryGetValue(key, out var warExhaustionRec) || Math.Max(warExhaustionRec.Faction1Value, warExhaustionRec.Faction2Value) < MaxWarExhaustion)
                return WarResult.None;

            return warExhaustionRec.VictoriousFaction switch
            {
                VictoriousFactionType.None => WarResult.None,
                VictoriousFactionType.Faction1 => kingdoms.ReversedKeyOrder ? WarResult.Loss : JudgeVictoryByWarExhaustion(warExhaustionRec.Faction1Value),
                VictoriousFactionType.Faction2 => kingdoms.ReversedKeyOrder ? JudgeVictoryByWarExhaustion(warExhaustionRec.Faction2Value) : WarResult.Loss,
                VictoriousFactionType.Both => WarResult.Tie,
                _ => WarResult.None
            };

            static WarResult JudgeVictoryByWarExhaustion(float warExhaustionValue) =>
                warExhaustionValue >= MaxWarExhaustion ? WarResult.PyrrhicVictory : IsCriticalWarExhaustion(warExhaustionValue) ? WarResult.CloseVictory : WarResult.Victory;
        }

        public static bool IsValidQuestState(Kingdom kingdom1, Kingdom kingdom2)
        {
            if (!IsValidCustomQuestState(kingdom1, kingdom2))
                return false;

            var currentStoryMode = StoryMode.StoryModeManager.Current;
            if (currentStoryMode == null)
            {
                // not in story mode
                return true;
            }

            var isValidQuestState = true;
            var opposingKingdom = PlayerHelper.GetOpposingKingdomIfPlayerKingdomProvided(kingdom1, kingdom2);

            if (opposingKingdom is not null)
            {
                var thirdPhase = currentStoryMode?.MainStoryLine?.ThirdPhase;
                isValidQuestState = thirdPhase is null || !thirdPhase.OppositionKingdoms.Contains(opposingKingdom);
            }

            return isValidQuestState;
        }

        private static bool IsValidCustomQuestState(Kingdom kingdom, Kingdom orherKingdom)
        {
            //TODO: Add an interface for other mods to affect this
            return true;
        }

        public void Sync()
        {
            Instance = this;
            //Too early to try gather war data on first load here, have to wait for OnAfterSaveLoaded
            _warExhaustionScores ??= new();
            _warExhaustionRates ??= new();
            _warExhaustionEventRecords ??= new();
        }

        public enum WarResult : byte
        {
            None = 0,
            Loss = 1,
            Tie = 2,
            PyrrhicVictory = 3,
            CloseVictory = 4,
            Victory = 5
        }

        public static WarResult Max(WarResult a, WarResult b) => a > b ? a : b;
        public static WarResult Min(WarResult a, WarResult b) => a < b ? a : b;
    }
}