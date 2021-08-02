using TaleWorlds.Localization;

namespace Diplomacy.CivilWar.Factions
{
    public enum RebelDemandType
    {
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
                    name = TextObject.Empty.ToString();
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
                    hint = new TextObject(
                            "{=LF19GulH}Withdraw from the kingdom. Members of this faction will form a new kingdom. Only Tier 4 or higher clans may start a secession faction.")
                        .ToString();
                    break;
                case RebelDemandType.Abdication:
                    hint = new TextObject(
                        "{=GK1seKeJ}Force the leader of the kingdom to step down. An election will decide the new leader of the kingdom.").ToString();
                    break;
                default:
                    hint = new TextObject().ToString();
                    break;
            }

            return hint;
        }
    }
}