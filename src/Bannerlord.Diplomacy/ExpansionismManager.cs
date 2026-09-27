using Diplomacy.Extensions;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace Diplomacy
{
    internal sealed class ExpansionismManager
    {
        [SaveableField(1)][UsedImplicitly] private Dictionary<IFaction, float> _expansionism;

        public static ExpansionismManager? Instance { get; private set; }
        public float SiegeExpansionism => Settings.Instance!.ExpanisonismPerSiege;
        public float ExpansionismDecayPerDay => Settings.Instance!.ExpansionismDecayPerDay;
        public float MinimumExpansionismPerFief => Settings.Instance!.MinimumExpansionismPerFief;
        public float CriticalExpansionism => Settings.Instance!.CriticalExpansionism;

        public ExpansionismManager()
        {
            _expansionism = new Dictionary<IFaction, float>();
            Instance = this;
        }

        public float GetMinimumExpansionism(Kingdom kingdom)
        {
            return kingdom.Fiefs.Count * MinimumExpansionismPerFief;
        }

        public float GetExpansionism(IFaction faction)
        {
            return _expansionism.TryGetValue(faction, out var result) ? result : 0f;
        }

        public void AddSiegeScore(IFaction faction)
        {
            _expansionism.TryGetValue(faction, out var value);
            _expansionism[faction] = Math.Max(value, GetMinimumExpansionism(faction) - MinimumExpansionismPerFief) + SiegeExpansionism;
        }

        /// <summary>
        /// Walks expansionism back down, floored at the minimum the faction's remaining fiefs
        /// imply. Handing a fief back is the only way a kingdom can actively undo the reputation
        /// it earned by taking one.
        /// </summary>
        public void ReduceExpansionism(IFaction faction, float amount)
        {
            if (amount <= 0f)
                return;

            _expansionism.TryGetValue(faction, out var value);
            _expansionism[faction] = Math.Max(value - amount, GetMinimumExpansionism(faction));
        }

        private static float GetMinimumExpansionism(IFaction faction)
        {
            return faction.IsKingdomFaction ? (faction as Kingdom)!.GetMinimumExpansionism() : default;
        }

        public void ApplyExpansionismDecay(IFaction faction)
        {
            if (_expansionism.TryGetValue(faction, out var value))
                _expansionism[faction] = Math.Max(value - ExpansionismDecayPerDay, GetMinimumExpansionism(faction));
            else
                _expansionism[faction] = GetMinimumExpansionism(faction);
        }

        internal void Sync()
        {
            Instance = this;
        }
    }
}
