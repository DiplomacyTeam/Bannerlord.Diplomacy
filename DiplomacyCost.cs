using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomacyFixes
{
    class DiplomacyCost
    {
        public DiplomacyCostType Type { get; }
        public float Value { get; }

        public DiplomacyCost(float value, DiplomacyCostType type)
        {
            this.Value = value;
            this.Type = type;
        }
    }

    public enum DiplomacyCostType
    {
        GOLD,
        INFLUENCE
    }
}
