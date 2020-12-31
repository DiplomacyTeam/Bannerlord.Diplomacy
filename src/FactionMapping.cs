using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace Diplomacy
{
    [SaveableStruct(9)]
    struct FactionMapping
    {
        [SaveableProperty(1)]
        internal IFaction Faction1 { get; private set; }

        [SaveableProperty(2)]
        internal IFaction Faction2 { get; private set; }

        [SaveableField(3)]
        private int _hashCode;

        internal FactionMapping(IFaction faction1, IFaction faction2)
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

        internal FactionMapping(FactionMapping other)
        {
            Faction1 = other.Faction1;
            Faction2 = other.Faction2;
            _hashCode = other._hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            FactionMapping FactionMapping = (FactionMapping)obj;
            return Faction1 == FactionMapping.Faction1 && Faction2 == FactionMapping.Faction2;
        }

        public bool Equals(FactionMapping p) => Faction1 == p.Faction1 && Faction2 == p.Faction2;

        public static bool operator ==(FactionMapping per1, FactionMapping per2) => per1.Equals(per2);

        public static bool operator !=(FactionMapping per1, FactionMapping per2) => !per1.Equals(per2);

        private static int CalculateHash(string s)
        {
            int num = 0;
            for (int i = 0; i < s.Length; i++)
            {
                num *= 17;
                num += (int)(char.ToUpper(s[i]) - '0');
            }
            return num;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}
