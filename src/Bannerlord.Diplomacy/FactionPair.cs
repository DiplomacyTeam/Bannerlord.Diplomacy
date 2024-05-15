using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace Diplomacy
{
    /// <summary>
    /// This is essentially an accessible copy of the game's own internal FactionPair class.
    /// </summary>
    internal struct FactionPair
    {
        [SaveableProperty(1)]
        internal IFaction Faction1 { get; init; }

        [SaveableProperty(2)]
        internal IFaction Faction2 { get; init; }

        [SaveableField(3)]
        private readonly int _hashCode;

        internal FactionPair(IFaction faction1, IFaction faction2)
        {
            if (string.CompareOrdinal(faction1.StringId, faction2.StringId) < 0)
            {
                Faction1 = faction1;
                Faction2 = faction2;
            }
            else
            {
                Faction1 = faction2;
                Faction2 = faction1;
            }

            _hashCode = CalculateHash(Faction1.StringId + Faction2.StringId);
        }

        internal FactionPair(FactionPair other)
        {
            Faction1 = other.Faction1;
            Faction2 = other.Faction2;
            _hashCode = other._hashCode;
        }

        public override bool Equals(object? obj) => obj is FactionPair p && Equals(p);

        public bool Equals(FactionPair p) => Faction1 == p.Faction1 && Faction2 == p.Faction2;

        public static bool operator ==(FactionPair p1, FactionPair p2) => p1.Equals(p2);

        public static bool operator !=(FactionPair p1, FactionPair p2) => !p1.Equals(p2);

        private static int CalculateHash(string s)
        {
            var num = 0;

            foreach (var t in s)
            {
                num *= 17;
                num += char.ToUpper(t) - '0';
            }

            return num;
        }

        public override int GetHashCode() => _hashCode;
    }
}