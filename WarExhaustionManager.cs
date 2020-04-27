using HarmonyLib;
using StoryMode.StoryModePhases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace DiplomacyFixes
{
    class WarExhaustionManager
    {
        private static WarExhaustionManager _instance;
        private Dictionary<HashSet<Kingdom>, float> _warExhaustion;
        private HashSet<HashSet<Kingdom>> _knownKingdomCombinations;
        private HashSet<Kingdom> _knownKingdoms;

        private const float MaxWarExhaustion = 100f;
        private const float MinWarExhaustion = 0f;

        public static WarExhaustionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WarExhaustionManager();
                }
                return _instance;
            }
        }

        private WarExhaustionManager()
        {
            this._warExhaustion = new Dictionary<HashSet<Kingdom>, float>(HashSet<Kingdom>.CreateSetComparer());
            HashSet<HashSet<Kingdom>> combinations = new HashSet<HashSet<Kingdom>>(HashSet<Kingdom>.CreateSetComparer());
            combinations.UnionWith(
                from item1 in Kingdom.All
                from item2 in Kingdom.All
                where String.Compare(item1.Name.ToString(), item2.Name.ToString()) < 0
                select new HashSet<Kingdom>() { item1, item2 });
            this._knownKingdoms = new HashSet<Kingdom>(Kingdom.All);
            this._knownKingdomCombinations = combinations;
        }

        public float GetWarExhaustion(Kingdom kingdom1, Kingdom kingdom2)
        {
            if (_warExhaustion.TryGetValue(new HashSet<Kingdom>() { kingdom1, kingdom2 }, out float warExhaustion))
            {
                return warExhaustion;
            }
            else
            {
                return 0f;
            }
        }

        private void AddWarExhaustion(HashSet<Kingdom> kingdoms)
        {
            if (_warExhaustion.TryGetValue(kingdoms, out float currentValue))
            {
                _warExhaustion[kingdoms] = MBMath.ClampFloat(currentValue += Settings.Instance.WarExhaustionMultiplier, MinWarExhaustion, MaxWarExhaustion);
            }
            else
            {
                _warExhaustion[kingdoms] = MBMath.ClampFloat(Settings.Instance.WarExhaustionMultiplier, MinWarExhaustion, MaxWarExhaustion);
            }
        }

        private void RemoveWarExhaustion(HashSet<Kingdom> kingdoms)
        {
            if (_warExhaustion.TryGetValue(kingdoms, out float currentValue))
            {
                _warExhaustion[kingdoms] = MBMath.ClampFloat(currentValue -= Settings.Instance.WarExhaustionMultiplier, MinWarExhaustion, MaxWarExhaustion);
            }
        }

        public void UpdateWarExhaustionForAllKingdoms()
        {
            UpdateKnownKingdoms();

            foreach (HashSet<Kingdom> kingdoms in _knownKingdomCombinations)
            {
                UpdateWarExhaustion(kingdoms);
            }
        }

        private void UpdateKnownKingdoms()
        {
            IEnumerable<Kingdom> kingdomsToAdd = Kingdom.All.Except(_knownKingdoms);
            if (kingdomsToAdd.Any())
            {
                _knownKingdomCombinations.UnionWith(
                from item1 in Kingdom.All
                from item2 in Kingdom.All
                where String.Compare(item1.Name.ToString(), item2.Name.ToString()) < 0
                select new HashSet<Kingdom>() { item1, item2 });
                _knownKingdoms.UnionWith(kingdomsToAdd);
            }
        }

        private void UpdateWarExhaustion(HashSet<Kingdom> kingdoms)
        {
            if (kingdoms.Count != 2)
            {
                throw new MBIllegalValueException("Can only update war exhaustion when there are two kingdoms provided.");
            }

            if (!IsValidQuestState(kingdoms))
            {
                return;
            }

            if (FactionManager.IsAtWarAgainstFaction(kingdoms.First(), kingdoms.First(element => element != kingdoms.First())))
            {
                AddWarExhaustion(kingdoms);
            }
            else
            {
                RemoveWarExhaustion(kingdoms);
            }
        }

        private bool IsValidQuestState(HashSet<Kingdom> kingdoms)
        {
            bool isValidQuestState = true;
            if (kingdoms.Any(kingdom => kingdom == Hero.MainHero.MapFaction))
            {
                ThirdPhase thirdPhase = StoryMode.StoryMode.Current.MainStoryLine.ThirdPhase;
                isValidQuestState = thirdPhase == null || thirdPhase.OppositionKingdoms.Intersect(kingdoms).IsEmpty();
            }
            return isValidQuestState;
        }
    }
}
