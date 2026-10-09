namespace Diplomacy
{
    internal sealed class Settings
    {
        public static Settings Instance { get; set; } = new();
        public bool EnablePlayerFactionRecruitment { get; set; } = true;
        public int FactionRecruitmentInfluenceCost { get; set; } = 25;
        public int FactionRecruitmentPersuasionBonus { get; set; } = 25;
        public int FactionRecruitmentCooldownInDays { get; set; } = 14;
        public int FactionRecruitmentPledgeInDays { get; set; } = 30;
        public bool EnableFactionRecruitmentRelationshipEffect { get; set; } = true;
        public int FactionRecruitmentRelationshipEffect { get; set; } = 25;
    }

    internal static class StringConstants
    {
        public const string NotEnoughInfluence = "Not enough influence";
    }
}
