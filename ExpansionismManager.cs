
using DiplomacyFixes.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes
{
    [SaveableClass(8)]
    class ExpansionismManager
    {
        [SaveableField(1)]
        private Dictionary<IFaction, float> _expansionism;

        public static ExpansionismManager Instance { get; private set; }
        public float SiegeExpansionism { get { return Settings.Instance.ExpanisonismPerSiege; } }
        public float ExpansionismDecayPerDay { get { return Settings.Instance.ExpansionismDecayPerDay; } }
        public float MinimumExpansionismPerFief { get { return Settings.Instance.MinimumExpansionismPerFief; } }
        public float CriticalExpansionism { get { return Settings.Instance.CriticalExpansionism; } }


        public float GetMinimumExpansionism(Kingdom kingdom)
        {
            return kingdom.Fiefs.Count() * MinimumExpansionismPerFief;
        }

        public ExpansionismManager()
        {
            this._expansionism = new Dictionary<IFaction, float>();
            Instance = this;
        }

        public float GetExpansionism(IFaction faction)
        {
            return this._expansionism.TryGetValue(faction, out float result) ? result : 0f;
        }

        public void AddSiegeScore(IFaction faction)
        {
            this._expansionism.TryGetValue(faction, out float value);
            this._expansionism[faction] = Math.Max(value, GetMinimumExpansionism(faction) - MinimumExpansionismPerFief) + SiegeExpansionism;
        }

        private static float GetMinimumExpansionism(IFaction faction)
        {
            return faction.IsKingdomFaction ? (faction as Kingdom).GetMinimumExpansionism() : default;
        }

        public void ApplyExpansionismDecay(IFaction faction)
        {
            if(this._expansionism.TryGetValue(faction, out float value))
            {
                float minimumExpansionism = faction.IsKingdomFaction ? (faction as Kingdom).GetMinimumExpansionism() : default;
                this._expansionism[faction] = Math.Max(value - ExpansionismDecayPerDay, GetMinimumExpansionism(faction));
            }
        }

        internal void Sync()
        {
            Instance = this;
        }
    }
}
