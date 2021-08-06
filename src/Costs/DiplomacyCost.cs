namespace Diplomacy.Costs
{
    public abstract class DiplomacyCost
    {
        public float Value { get; }

        protected DiplomacyCost(float value)
        {
            Value = value;
        }

        public abstract void ApplyCost();
        public abstract bool CanPayCost();
    }
}
