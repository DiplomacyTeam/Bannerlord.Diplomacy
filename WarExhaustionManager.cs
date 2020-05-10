using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes
{
    [SaveableClass(2)]
    class WarExhaustionManager
    {

        // legacy war exhaustion dictionary using stringId
        [SaveableField(0)]
        private Dictionary<string, float> _warExhaustion;

        // new war exhaustion dictionary using Id
        [SaveableField(1)]
        private Dictionary<string, float> _warExhaustionById;

        private HashSet<Tuple<Kingdom, Kingdom>> _knownKingdomCombinations;
        private HashSet<Kingdom> _knownKingdoms;

        public static WarExhaustionManager Instance { get; internal set; }

        internal static float MaxWarExhaustion { get { return Settings.Instance.MaxWarExhaustion; } }
        internal static float MinWarExhaustion { get { return 0f; } }


        private float Fuzziness { get { return MBRandom.RandomFloatRanged(BaseFuzzinessMin, BaseFuzzinessMax); } }

        private float BaseFuzzinessMin { get { return 0.5f; } }
        private float BaseFuzzinessMax { get { return 1.5f; } }

        public static float DefaultMaxWarExhaustion { get; } = 100f;

        internal WarExhaustionManager()
        {
            this._warExhaustionById = new Dictionary<string, float>();
            this._knownKingdomCombinations = new HashSet<Tuple<Kingdom, Kingdom>>();
            this._knownKingdoms = new HashSet<Kingdom>();
            Instance = this;
        }

        public float GetWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            string key = CreateKey(kingdom1, kingdom2);
            if (key != null && _warExhaustionById.TryGetValue(key, out float warExhaustion))
            {
                return warExhaustion;
            }
            else
            {
                return 0f;
            }
        }

        private static bool KingdomsAreValid(Kingdom kingdom1, Kingdom kingdom2)
        {
            return kingdom1.Id != null && kingdom2.Id != null;
        }

        private string CreateKey(Kingdom kingdom1, Kingdom kingdom2)
        {
            return CreateKey(new Tuple<Kingdom, Kingdom>(kingdom1, kingdom2));
        }

        private string CreateKey(Tuple<Kingdom, Kingdom> kingdoms)
        {
            if (KingdomsAreValid(kingdoms.Item1, kingdoms.Item2))
            {
                return string.Join("+", kingdoms.Item1.Id, kingdoms.Item2.Id);
            }
            else
            {
                return null;
            }
        }

        private void AddDailyWarExhaustion(Tuple<Kingdom, Kingdom> kingdoms)
        {
            float warExhaustionToAdd = GetDailyWarExhaustionDelta();
            AddWarExhaustion(kingdoms.Item1, kingdoms.Item2, warExhaustionToAdd);
        }

        public void AddCasualtyWarExhaustion(Kingdom kingdom1, Kingdom kingdom2, int casualties)
        {
            float warExhaustionToAdd = Settings.Instance.WarExhaustionPerCasualty * casualties;
            AddWarExhaustion(kingdom1, kingdom2, warExhaustionToAdd);
        }

        public void AddSiegeWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            float warExhaustionToAdd = Settings.Instance.WarExhaustionPerSiege;
            AddWarExhaustion(kingdom1, kingdom2, warExhaustionToAdd);
        }

        public void AddRaidWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            float warExhaustionToAdd = Settings.Instance.WarExhaustionPerRaid;
            AddWarExhaustion(kingdom1, kingdom2, warExhaustionToAdd);
        }

        private void AddWarExhaustion(Kingdom kingdom1, Kingdom kingdom2, float warExhaustionToAdd)
        {
            string key = CreateKey(kingdom1, kingdom2);
            if (key != null)
            {
                AddWarExhaustion(key, warExhaustionToAdd, true);
            }
        }

        private void AddWarExhaustion(string key, float warExhaustionToAdd, bool addFuzziness)
        {
            float finalWarExhaustionDelta = warExhaustionToAdd;
            if (addFuzziness)
            {
                finalWarExhaustionDelta *= Fuzziness;
            }
            if (_warExhaustionById.TryGetValue(key, out float currentValue))
            {
                _warExhaustionById[key] = MBMath.ClampFloat(currentValue += finalWarExhaustionDelta, MinWarExhaustion, MaxWarExhaustion);
            }
            else
            {
                _warExhaustionById[key] = MBMath.ClampFloat(finalWarExhaustionDelta, MinWarExhaustion, MaxWarExhaustion);
            }
        }

        private float GetDailyWarExhaustionDelta()
        {
            return Settings.Instance.WarExhaustionPerDay;
        }

        private void RemoveDailyWarExhaustion(Tuple<Kingdom, Kingdom> kingdoms)
        {
            string key = CreateKey(kingdoms);
            if (key != null && _warExhaustionById.TryGetValue(key, out float currentValue))
            {
                float warExhaustionToRemove = Settings.Instance.WarExhaustionDecayPerDay;
                _warExhaustionById[key] = MBMath.ClampFloat(currentValue -= warExhaustionToRemove, MinWarExhaustion, MaxWarExhaustion);
            }
        }

        public void UpdateDailyWarExhaustionForAllKingdoms()
        {
            UpdateKnownKingdoms();

            foreach (Tuple<Kingdom, Kingdom> kingdoms in _knownKingdomCombinations)
            {
                UpdateDailyWarExhaustion(kingdoms);
            }
        }

        private void UpdateKnownKingdoms()
        {
            IEnumerable<Kingdom> kingdomsToAdd;
            if (_knownKingdoms == null)
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

            if (FactionManager.IsAtWarAgainstFaction(kingdoms.Item1, kingdoms.Item2))
            {
                IEnumerable<CampaignWar> campaignWars = Campaign.Current.FactionManager.FindCampaignWarsBetweenFactions(kingdoms.Item1, kingdoms.Item2);
                CampaignWar campaignWar = null;
                if (campaignWars?.Any() == true)
                {
                    campaignWar = (from war in campaignWars
                                   orderby war.StartDate
                                   select war).FirstOrDefault();
                }
                if (campaignWar == null || (float)Math.Round(campaignWar.StartDate.ElapsedDaysUntilNow) >= 1.0f)
                {
                    AddDailyWarExhaustion(kingdoms);
                }
            }
            else
            {
                RemoveDailyWarExhaustion(kingdoms);
            }
        }

        public bool HasMaxWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            return GetWarExhaustion(kingdom1, kingdom2) >= MaxWarExhaustion;
        }

        public void Sync()
        {
            Instance = this;
            if (this._warExhaustionById == null)
            {
                this._warExhaustionById = new Dictionary<string, float>();
            }
            MigrateLegacyWarExhaustionDictionary();
        }


        /// <summary>
        /// Migrates old-style war exhaustion dictionaries that are keyed by stringId to new ones keyed by MBGUID internal id.
        /// </summary>
        private void MigrateLegacyWarExhaustionDictionary()
        {
            if (_warExhaustion != null)
            {
                foreach (string oldKey in _warExhaustion.Keys)
                {
                    string[] kingdomNames = oldKey.Split(new char[] { '+' });
                    if (kingdomNames.Length != 2)
                    {
                        continue;
                    }
                    Kingdom kingdom1 = Kingdom.All.FirstOrDefault(kingdom => kingdomNames[0] == kingdom.StringId);
                    Kingdom kingdom2 = Kingdom.All.FirstOrDefault(kingdom => kingdomNames[1] == kingdom.StringId);

                    if (kingdom1 == null || kingdom2 == null)
                    {
                        continue;
                    }

                    string newKey = CreateKey(kingdom1, kingdom2);
                    if (newKey != null)
                    {
                        _warExhaustionById[newKey] = _warExhaustion[oldKey];
                    }
                }
                _warExhaustion = null;
            }
        }
    }
}
