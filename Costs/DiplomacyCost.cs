using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomacyFixes
{
    abstract class DiplomacyCost
    {
        public float Value { get; }

        public DiplomacyCost(float value)
        {
            this.Value = value;
        }

        public abstract void ApplyCost();
        public abstract bool CanPayCost();
    }
}
