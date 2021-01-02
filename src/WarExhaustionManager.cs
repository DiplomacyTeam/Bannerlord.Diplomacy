using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace Diplomacy
{
    [SaveableClass(2)]
    class WarExhaustionManager
    {

        // legacy war exhaustion dictionary using stringId
        [SaveableField(0)]
        private Dictionary<string, float>? _warExhaustion;

        // new war exhaustion dictionary using Id
        [SaveableField(1)]
        private Dictionary<string, float> _warExhaustionById;

        private HashSet<Tuple<Kingdom, Kingdom>> _knownKingdomCombinations;
        private HashSet<Kingdom> _knownKingdoms;

        public static WarExhaustionManager Instance { get; private set; } = default!;

        internal static float MaxWarExhaustion => Settings.Instance!.MaxWarExhaustion;

        internal static float MinWarExhaustion => 0f;


        private float Fuzziness => MBRandom.RandomFloatRanged(BaseFuzzinessMin, BaseFuzzinessMax);

        private float BaseFuzzinessMin => 0.5f;

        private float BaseFuzzinessMax => 1.5f;

        public static float DefaultMaxWarExhaustion { get; } = 100f;

        internal WarExhaustionManager()
        {
            _warExhaustionById = new Dictionary<string, float>();
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

        private void AddWarExhaustion(Kingdom kingdom1, Kingdom kingdom2, float warExhaustionToAdd, WarExhaustionType warExhaustionType, bool addFuzziness = true)
        {
            var key = CreateKey(kingdom1, kingdom2);
            if (key is not null)
            {
                var finalWarExhaustionDelta = warExhaustionToAdd;
                if (addFuzziness)
                {
                    finalWarExhaustionDelta *= Fuzziness;
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
            }
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
            MigrateLegacyWarExhaustionDictionary();
        }


        /// <summary>
        /// Migrates old-style war exhaustion dictionaries that are keyed by stringId to new ones keyed by MBGUID internal id.
        /// </summary>
        private void MigrateLegacyWarExhaustionDictionary()
        {
            if (_warExhaustion is not null)
            {
                foreach (var oldKey in _warExhaustion.Keys)
                {
                    var kingdomNames = oldKey.Split(new char[] { '+' });

                    if (kingdomNames.Length != 2)
                    {
                        continue;
                    }

                    var kingdom1 = Kingdom.All.FirstOrDefault(kingdom => kingdomNames[0] == kingdom.StringId);
                    var kingdom2 = Kingdom.All.FirstOrDefault(kingdom => kingdomNames[1] == kingdom.StringId);

                    if (kingdom1 is null || kingdom2 is null)
                    {
                        continue;
                    }

                    var newKey = CreateKey(kingdom1, kingdom2);

                    if (newKey is not null)
                    {
                        _warExhaustionById[newKey] = _warExhaustion[oldKey];
                    }
                }
                _warExhaustion = null;
            }
        }

        private enum WarExhaustionType
        {
            Casualty,
            Raid,
            Siege,
            Daily
        }
    }
}
