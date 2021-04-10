
using TaleWorlds.Localization;

namespace Diplomacy.CivilWar
{
    public enum RebelDemandType
    {
        None,
        Secession,
        Abdication
    }

    public static class RebelDemandStringHelper 
    {
        public static string GetName(this RebelDemandType rebelDemandType)
        {
            string name;
            switch (rebelDemandType)
            {
                case RebelDemandType.Secession:
                    name = new TextObject("{=ZAfNviVS}Secession").ToString();
                    break;
                case RebelDemandType.Abdication:
                    name = new TextObject("{=wUspUo1y}Abdication").ToString();
                    break;
                default:
                    name = new TextObject("").ToString();
                    break;
            }
            return name;
        }

        public static string GetHint(this RebelDemandType rebelDemandType)
        {
            string hint;
            switch (rebelDemandType)
            {
                case RebelDemandType.Secession:
                    hint = new TextObject("{=LF19GulH}Withdraw from the kingdom. Members of this faction will form a new kingdom.").ToString();
                    break;
                case RebelDemandType.Abdication:
                    hint = new TextObject("{=GK1seKeJ}Force the leader of the kingdom to step down. An election will decide the new leader of the kingdom.").ToString();
                    break;
                default:
                    hint = new TextObject("").ToString();
                    break;
            }
            return hint;
        }
    }
}
