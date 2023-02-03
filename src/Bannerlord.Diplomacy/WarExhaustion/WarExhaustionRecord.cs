using System;

using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace Diplomacy.WarExhaustion
{
    public readonly struct WarExhaustionRecord
    {
        [SaveableProperty(1)]
        public float Faction1Value { get; init; }
        [SaveableProperty(2)]
        public float Faction2Value { get; init; }
        [SaveableProperty(3)]
        public VictoriousFactionType VictoriousFaction { get; init; }
        [SaveableProperty(4)]
        public ActiveQuestState QuestState { get; init; }

        internal DerivedRecordStats RecordStats { get; init; }

        public WarExhaustionRecord(float faction1Value, float faction2Value, VictoriousFactionType victoriousFaction = VictoriousFactionType.None, bool hasActiveQuest = false, bool considerRangeLimits = true)
            : this(faction1Value, faction2Value, victoriousFaction, hasActiveQuest ? ActiveQuestState.HasActiveQuest : ActiveQuestState.None, considerRangeLimits) { }

        internal WarExhaustionRecord(float faction1Value, float faction2Value, VictoriousFactionType victoriousFaction, ActiveQuestState activeQuest, bool considerRangeLimits = true)
        {
            GetValueRangeLimits(activeQuest == ActiveQuestState.HasActiveQuest, considerRangeLimits, out var minScore, out var maxScore, out var effectiveMaxScore);

            Faction1Value = MBMath.ClampFloat(faction1Value, minScore, effectiveMaxScore);
            Faction2Value = MBMath.ClampFloat(faction2Value, minScore, effectiveMaxScore);
            VictoriousFaction = CalculateVictorIfApplicable(faction1Value, faction2Value, victoriousFaction, maxScore);

            QuestState = activeQuest;
            RecordStats = (Faction1Value != faction1Value, Faction2Value != faction2Value, VictoriousFaction != victoriousFaction);
        }

        private static void GetValueRangeLimits(bool hasActiveQuest, bool considerRangeLimits, out float minScore, out float maxScore, out float effectiveMaxScore)
        {
            if (considerRangeLimits)
            {
                minScore = WarExhaustionManager.MinWarExhaustion;
                maxScore = WarExhaustionManager.MaxWarExhaustion;
                effectiveMaxScore = hasActiveQuest ? maxScore * WarExhaustionManager.CriticalThresholdWarExhaustion : maxScore;
            }
            else
            {
                minScore = float.MinValue;
                maxScore = float.MaxValue;
                effectiveMaxScore = float.MaxValue;
            }
        }

        private static VictoriousFactionType CalculateVictorIfApplicable(float faction1Value, float faction2Value, VictoriousFactionType victoriousFaction, float maxScore)
        {
            var newVictoriousFaction = victoriousFaction;
            if (newVictoriousFaction == VictoriousFactionType.None)
            {
                if (faction1Value >= maxScore && faction2Value < maxScore)
                    newVictoriousFaction = VictoriousFactionType.Faction2;
                else if (faction2Value >= maxScore && faction1Value < maxScore)
                    newVictoriousFaction = VictoriousFactionType.Faction1;
                else if (faction1Value >= maxScore && faction2Value >= maxScore)
                    newVictoriousFaction = VictoriousFactionType.Both;
            }
            return newVictoriousFaction;
        }

        public static ActiveQuestState Max(ActiveQuestState a, ActiveQuestState b) => a > b ? a : b;
        public static ActiveQuestState Min(ActiveQuestState a, ActiveQuestState b) => a < b ? a : b;

        private static VictoriousFactionType GetAppropriateVictoriousFaction(VictoriousFactionType a, VictoriousFactionType b) => (a > VictoriousFactionType.None) ? a : b;
        private static ActiveQuestState GetAppropriateQuestState(ActiveQuestState a, ActiveQuestState b) => (a > ActiveQuestState.None) ? Max(b, ActiveQuestState.HadActiveQuest) : b;

        public override string ToString() => string.Format("({0},{1})", Faction1Value.ToString("F2"), Faction2Value.ToString("F2"));
        public override bool Equals(object obj) => obj is WarExhaustionRecord warExhaustionRecord && Equals(warExhaustionRecord);
        public bool Equals(WarExhaustionRecord warExhaustionRecord) => Faction1Value == warExhaustionRecord.Faction1Value && Faction2Value == warExhaustionRecord.Faction2Value;
        public override int GetHashCode() => Faction1Value.GetHashCode() ^ Faction2Value.GetHashCode();
        public static WarExhaustionRecord operator +(WarExhaustionRecord a) => a;
        public static WarExhaustionRecord operator -(WarExhaustionRecord a) => new(-a.Faction1Value, -a.Faction2Value, a.VictoriousFaction, a.QuestState);
        public static WarExhaustionRecord operator +(WarExhaustionRecord a, WarExhaustionRecord b) =>
            new(a.Faction1Value + b.Faction1Value, a.Faction2Value + b.Faction2Value, GetAppropriateVictoriousFaction(a.VictoriousFaction, b.VictoriousFaction), GetAppropriateQuestState(a.QuestState, b.QuestState));
        public static WarExhaustionRecord operator -(WarExhaustionRecord a, WarExhaustionRecord b) => a + (-b);
        public static WarExhaustionRecord operator *(WarExhaustionRecord a, WarExhaustionRecord b) =>
            new(a.Faction1Value * b.Faction1Value, a.Faction2Value * b.Faction2Value, GetAppropriateVictoriousFaction(a.VictoriousFaction, b.VictoriousFaction), GetAppropriateQuestState(a.QuestState, b.QuestState));
        public static WarExhaustionRecord operator /(WarExhaustionRecord a, WarExhaustionRecord b)
        {
            if (b.Faction1Value == 0 || b.Faction2Value == 0)
            {
                throw new DivideByZeroException();
            }
            return new(a.Faction1Value / b.Faction1Value, a.Faction2Value / b.Faction2Value, GetAppropriateVictoriousFaction(a.VictoriousFaction, b.VictoriousFaction), GetAppropriateQuestState(a.QuestState, b.QuestState));
        }
        public static WarExhaustionRecord ChangeQuestState(WarExhaustionRecord a, bool hasActiveQuest) => new(a.Faction1Value, a.Faction2Value, a.VictoriousFaction, GetAppropriateQuestState(a.QuestState, hasActiveQuest ? ActiveQuestState.HasActiveQuest : ActiveQuestState.None));

        internal readonly struct DerivedRecordStats
        {
            public readonly bool Faction1ScoreClamped;
            public readonly bool Faction2ScoreClamped;
            public readonly bool DefinesVictor;

            public DerivedRecordStats(bool faction1ScoreClamped, bool faction2ScoreClamped, bool definesVictor)
            {
                Faction1ScoreClamped = faction1ScoreClamped;
                Faction2ScoreClamped = faction2ScoreClamped;
                DefinesVictor = definesVictor;
            }

            public override bool Equals(object? obj)
            {
                return obj is DerivedRecordStats other &&
                       Faction1ScoreClamped == other.Faction1ScoreClamped &&
                       Faction2ScoreClamped == other.Faction2ScoreClamped &&
                       DefinesVictor == other.DefinesVictor;
            }

            public override int GetHashCode() => HashCode.Combine(Faction1ScoreClamped, Faction2ScoreClamped, DefinesVictor);

            public void Deconstruct(out bool faction1ScoreClamped, out bool faction2ScoreClamped, out bool definesVictor)
            {
                faction1ScoreClamped = Faction1ScoreClamped;
                faction2ScoreClamped = Faction2ScoreClamped;
                definesVictor = DefinesVictor;
            }

            public static implicit operator (bool Faction1ScoreClamped, bool Faction2ScoreClamped, bool definesVictor)(DerivedRecordStats value) => (value.Faction1ScoreClamped, value.Faction2ScoreClamped, value.DefinesVictor);

            public static implicit operator DerivedRecordStats((bool Faction1ScoreClamped, bool Faction2ScoreClamped, bool definesVictor) value) => new(value.Faction1ScoreClamped, value.Faction2ScoreClamped, value.definesVictor);
        }

        [Flags]
        public enum VictoriousFactionType : byte
        {
            None = 0,
            Faction1 = 1,
            Faction2 = 2,
            Both = Faction1 | Faction2
        }

        public enum ActiveQuestState : byte
        {
            None = 0,
            HadActiveQuest = 1,
            HasActiveQuest = 2
        }
    }
}