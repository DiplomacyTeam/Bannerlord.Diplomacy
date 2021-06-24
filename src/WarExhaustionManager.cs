using Diplomacy.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace Diplomacy
{
    internal sealed class WarExhaustionManager
    {

        // new war exhaustion dictionary using Id
        [SaveableField(1)]
        private Dictionary<string, float> _warExhaustionById;

        [SaveableField(2)]
        private Dictionary<string, float> _warExhaustionMultiplier;

        private HashSet<Tuple<Kingdom, Kingdom>> _knownKingdomCombinations;
        private HashSet<Kingdom> _knownKingdoms;

        public static WarExhaustionManager Instance { get; private set; } = default!;

        internal static float MaxWarExhaustion => 100f;

        internal static float MinWarExhaustion => 0f;

        public static float DefaultMaxWarExhaustion { get; } = 100f;

        internal WarExhaustionManager()
        {
            _warExhaustionById = new Dictionary<string, float>();
            _warExhaustionMultiplier = new Dictionary<string, float>();
            _knownKingdomCombinations = new HashSet<Tuple<Kingdom, Kingdom>>();
            _knownKingdoms = new HashSet<Kingdom>();
            Instance = this;
        }

        public float GetWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            var key = CreateKey(kingdom1, kingdom2);
            return key is not null && _warExhaustionById.TryGetValue(key, out var warExhaustion)
                ? warExhaustion
                : 0f;
        }

        internal void RegisterWarExhaustionMultiplier(Kingdom kingdom1, Kingdom kingdom2)
        {
            float average = (kingdom1.TotalStrength + kingdom2.TotalStrength) / 2;
            var multiplier = 1000f / average;

            var key = CreateKey(kingdom1, kingdom2);
            var key2 = CreateKey(kingdom2, kingdom1);
            if (key is not null && key2 is not null)
            {
                _warExhaustionMultiplier![key!] = multiplier;
                _warExhaustionMultiplier![key2!] = multiplier;
            }
        }

        private static bool KingdomsAreValid(Kingdom kingdom1, Kingdom kingdom2)
            => kingdom1 is not null && kingdom2 is not null && kingdom1.Id != default && kingdom2.Id != default;

        private string? CreateKey(Kingdom kingdom1, Kingdom kingdom2) => CreateKey(new Tuple<Kingdom, Kingdom>(kingdom1, kingdom2));

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
                float warExhaustionFactor;
                if (!_warExhaustionMultiplier!.TryGetValue(key, out warExhaustionFactor))
                {
                    RegisterWarExhaustionMultiplier(kingdom1, kingdom2);
                    warExhaustionFactor = _warExhaustionMultiplier[key];
                }

                finalWarExhaustionDelta = warExhaustionToAdd * warExhaustionFactor;
            }

            if (_warExhaustionById.TryGetValue(key, out var currentValue))
            {
                _warExhaustionById[key] = MBMath.ClampFloat(currentValue += finalWarExhaustionDelta, MinWarExhaustion, MaxWarExhaustion);
            }
            else
            {
                _warExhaustionById[key] = MBMath.ClampFloat(finalWarExhaustionDelta, MinWarExhaustion, MaxWarExhaustion);
            }

            if (Settings.Instance!.EnableWarExhaustionDebugMessages && kingdom1 == Hero.MainHero.MapFaction)
            {
                var information = string.Format("Added {0} {1} war exhaustion to {2}'s war with {3}", finalWarExhaustionDelta, Enum.GetName(typeof(WarExhaustionType), warExhaustionType), kingdom1.Name, kingdom2.Name);
                InformationManager.DisplayMessage(new InformationMessage(information, Color.FromUint(4282569842U)));
            }

            Events.Instance.OnWarExhaustionAdded(new WarExhaustionEvent(kingdom1, kingdom2, warExhaustionType, warExhaustionToAdd));
        }

        private float GetDailyWarExhaustionDelta() => Settings.Instance!.WarExhaustionPerDay;

        private void RemoveDailyWarExhaustion(Tuple<Kingdom, Kingdom> kingdoms)
        {
            var key = CreateKey(kingdoms);
            if (key is not null && _warExhaustionById.TryGetValue(key, out var currentValue))
            {
                var warExhaustionToRemove = Settings.Instance!.WarExhaustionDecayPerDay;
                _warExhaustionById[key] = MBMath.ClampFloat(currentValue -= warExhaustionToRemove, MinWarExhaustion, MaxWarExhaustion);
            }
        }

        public void UpdateDailyWarExhaustionForAllKingdoms()
        {
            UpdateKnownKingdoms();

            foreach (var kingdoms in _knownKingdomCombinations)
            {
                UpdateDailyWarExhaustion(kingdoms);
            }
        }

        private void UpdateKnownKingdoms()
        {
            IEnumerable<Kingdom> kingdomsToAdd;

            if (_knownKingdoms is null)
            {
                _knownKingdoms = new HashSet<Kingdom>();
                _knownKingdomCombinations = new HashSet<Tuple<Kingdom, Kingdom>>();
                kingdomsToAdd = Kingdom.All;
            }
            else
            {
                kingdomsToAdd = Kingdom.All.Except(_knownKingdoms);
            }

            if (kingdomsToAdd.Any())
            {
                _knownKingdomCombinations.UnionWith(
                from item1 in Kingdom.All
                from item2 in Kingdom.All
                where item1.Id != item2.Id
                select new Tuple<Kingdom, Kingdom>(item1, item2));
                _knownKingdoms.UnionWith(kingdomsToAdd);
            }
        }

        private void UpdateDailyWarExhaustion(Tuple<Kingdom, Kingdom> kingdoms)
        {

            var stanceLink = kingdoms.Item1.GetStanceWith(kingdoms.Item2);
            if (stanceLink?.IsAtWar ?? false && (float)Math.Round(stanceLink.WarStartDate.ElapsedDaysUntilNow) >= 1.0f)
            {
                AddDailyWarExhaustion(kingdoms);
            }
            else
            {
                RemoveDailyWarExhaustion(kingdoms);
            }
        }

        public bool HasMaxWarExhaustion(Kingdom kingdom1, Kingdom kingdom2) => GetWarExhaustion(kingdom1, kingdom2) >= MaxWarExhaustion;

        public bool HasLowWarExhaustion(Kingdom kingdom1, Kingdom kingdom2) => GetWarExhaustion(kingdom1, kingdom2) <= GetLowWarExhaustion();

        public static double GetLowWarExhaustion() => 0.5 * MaxWarExhaustion;

        public void Sync()
        {
            Instance = this;
            _warExhaustionById ??= new();
            _warExhaustionMultiplier ??= new();
        }

        internal enum WarExhaustionType
        {
            Casualty,
            Raid,
            Siege,
            Daily
        }
    }
}
