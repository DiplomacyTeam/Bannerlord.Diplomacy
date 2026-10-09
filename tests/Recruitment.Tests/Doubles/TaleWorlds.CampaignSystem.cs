// Only the recruitment rules, records, membership checks and influence costs are production code.
// These minimal campaign objects allow testing them without running Bannerlord's native engine.
namespace TaleWorlds.CampaignSystem
{
    public sealed class Hero
    {
        public static Hero MainHero { get; set; } = null!;
        public static Hero? OneToOneConversationHero { get; set; }
        public float RelationWithPlayer { get; set; }
        public float GetRelationWithPlayer() => RelationWithPlayer;
        public bool IsActive { get; set; } = true;
        public bool IsPrisoner { get; set; }
    }

    public sealed class Clan
    {
        public static Clan PlayerClan { get; set; } = null!;
        public Hero Leader { get; set; } = new();
        public Kingdom Kingdom { get; set; } = null!;
        public bool IsEliminated { get; set; }
        public bool IsUnderMercenaryService { get; set; }
        public bool IsMinorFaction { get; set; }
        public float Influence { get; set; }
        public float Support { get; set; }
    }

    public sealed class Kingdom
    {
        public Clan RulingClan { get; set; } = null!;
        public bool IsEliminated { get; set; }
        public bool Rebel { get; set; }
        public List<Diplomacy.CivilWar.Factions.RebelFaction> Factions { get; } = new();
    }

    public readonly struct CampaignTime
    {
        public static float NowDays { get; set; }
        private readonly float _days;
        private CampaignTime(float days) => _days = days;
        public float ElapsedDaysUntilNow => NowDays - _days;
        public static CampaignTime Zero => new(0);
        public static CampaignTime DaysFromNow(float days) => new(NowDays + days);
    }

    public sealed class CharacterObject { }
}
