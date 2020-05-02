using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes
{
    [SaveableClass(2)]
    class WarExhaustionManager
    {

        [SaveableField(0)]
        private Dictionary<string, float> _warExhaustion;
        private HashSet<Tuple<Kingdom, Kingdom>> _knownKingdomCombinations;
        private HashSet<Kingdom> _knownKingdoms;

        internal static float MaxWarExhaustion { get { return Settings.Instance.MaxWarExhaustion; } }
        internal static float MinWarExhaustion { get { return 0f; } }


        private float Fuzziness { get { return MBRandom.RandomFloatRanged(BaseFuzzinessMin, BaseFuzzinessMax); } }

        private float BaseFuzzinessMin { get { return 0.5f; } }
        private float BaseFuzzinessMax { get { return 1.5f; } }

        internal WarExhaustionManager()
        {
            this._warExhaustion = new Dictionary<string, float>();
            this._knownKingdomCombinations = new HashSet<Tuple<Kingdom, Kingdom>>();
            this._knownKingdoms = new HashSet<Kingdom>();
        }

        public float GetWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {

            if (_warExhaustion.TryGetValue(CreateKey(kingdom1, kingdom2), out float warExhaustion))
            {
                return warExhaustion;
            }
            else
            {
                return 0f;
            }
        }

        private string CreateKey(Kingdom kingdom1, Kingdom kingdom2)
        {
            return CreateKey(new Tuple<Kingdom, Kingdom>(kingdom1, kingdom2));
        }

        private string CreateKey(Tuple<Kingdom, Kingdom> kingdoms)
        {
            return string.Join("+", kingdoms.Item1.StringId, kingdoms.Item2.StringId);
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
            AddWarExhaustion(CreateKey(kingdom1, kingdom2), warExhaustionToAdd, true);
        }

        private void AddWarExhaustion(string key, float warExhaustionToAdd, bool addFuzziness)
        {
            float finalWarExhaustionDelta = warExhaustionToAdd;
            if (addFuzziness)
            {
                finalWarExhaustionDelta *= Fuzziness;
            }
            if (_warExhaustion.TryGetValue(key, out float currentValue))
            {
                _warExhaustion[key] = MBMath.ClampFloat(currentValue += finalWarExhaustionDelta, MinWarExhaustion, MaxWarExhaustion);
            }
            else
            {
                _warExhaustion[key] = MBMath.ClampFloat(finalWarExhaustionDelta, MinWarExhaustion, MaxWarExhaustion);
            }
        }

        private float GetDailyWarExhaustionDelta()
        {
            return Settings.Instance.WarExhaustionPerDay;
        }

        private void RemoveDailyWarExhaustion(Tuple<Kingdom, Kingdom> kingdoms)
        {
            string key = CreateKey(kingdoms);
            if (_warExhaustion.TryGetValue(key, out float currentValue))
            {
                float warExhaustionToRemove = Settings.Instance.WarExhaustionDecayPerDay;
                _warExhaustion[key] = MBMath.ClampFloat(currentValue -= warExhaustionToRemove, MinWarExhaustion, MaxWarExhaustion);
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
                where String.Compare(item1.Name.ToString(), item2.Name.ToString()) != 0
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
    }
}
