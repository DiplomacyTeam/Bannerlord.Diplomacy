namespace Diplomacy.Costs
{
    public interface IDiplomacyCost
    {
        public abstract void ApplyCost();
        public abstract bool CanPayCost();
    }
}