namespace Diplomacy
{
    abstract class DiplomacyCost
    {
        public float Value { get; }

        public DiplomacyCost(float value)
        {
            Value = value;
        }

        public abstract void ApplyCost();
        public abstract bool CanPayCost();
    }
}
