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