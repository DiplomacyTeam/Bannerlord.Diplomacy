using MBOptionScreen.Attributes;
using MBOptionScreen.Settings;

namespace DiplomacyFixes
{
    class Settings : AttributeSettings<Settings>
    {
        public override string Id { get; set; } = "DiplomacyFixesSettings_1";

        public override string ModuleFolderName { get; } = "DiplomacyFixes";

        public override string ModName { get; } = "Diplomacy Fixes";

        [SettingProperty("Enable Player Kingdom Diplomacy Control", 0, 500, false, "Gives the player total control over their kingdom's war and peace declarations.")]
        [SettingPropertyGroup("Kingdom Diplomacy", false)]
        public bool PlayerDiplomacyControl { get; set; } = true;

        [SettingProperty("Enable Influence Costs", false, "If disabled, this removes all costs for war and peace declaration actions. Default value is true.")]
        [SettingPropertyGroup("Influence Costs", true)]
        public bool EnableInfluenceCostsForDiplomacyActions { get; set; } = true;

        [SettingProperty("Enable Scaling Influence Cost", false, "If enabled, this will scale influence costs based on your kingdom size. Otherwise, flat influence costs are used. Default value is true.")]
        [SettingPropertyGroup("Influence Costs", false)]
        public bool ScalingInfluenceCosts { get; set; } = true;

        [SettingProperty("Scaling Influence Cost Multiplier", 0f, 100f, false, "Multiplier for the scaling influence costs. Default value is 5.")]
        [SettingPropertyGroup("Influence Costs", false)]
        public float ScalingInfluenceCostMultiplier { get; set; } = 5.0f;

        [SettingProperty("Flat Declare War Influence Cost", 0, 10000, false, "Influence cost for declaring war on another kingdom. Default value is 100.")]
        [SettingPropertyGroup("Influence Costs", false)]
        public int DeclareWarInfluenceCost { get; set; } = 100;

        [SettingProperty("Flat Make Peace Influence Cost", 0, 10000, false, "Influence cost for making peace with another kingdom. Default value is 100.")]
        [SettingPropertyGroup("Influence Costs", false)]
        public int MakePeaceInfluenceCost { get; set; } = 100;

        [SettingProperty("Minimum War Duration in Days", 0, 500, false, "The minimum duration (in days) that a war can last before proposing peace. Default value is 10.")]
        [SettingPropertyGroup("Kingdom Diplomacy", false)]
        public int MinimumWarDurationInDays { get; set; } = 10;

        [SettingProperty("Declare War Cooldown in Days", 0, 500, false, "The minimum duration (in days) to declare war after making peace. Default value is 10.")]
        [SettingPropertyGroup("Kingdom Diplomacy", false)]
        public int DeclareWarCooldownInDays { get; set; } = 10;

        [SettingProperty("Send Messenger Influence Cost", 0, 10000, false, "Influence cost for sending a messenger to another leader. Default value is 20.")]
        [SettingPropertyGroup("Messengers", false)]
        public int SendMessengerInfluenceCost { get; set; } = 100;

        [SettingProperty("Messenger Travel Time in Days", 0, 500, false, "The amount of time (in days) a messenger takes to reach a kingdom's leader. Default value is 3.")]
        [SettingPropertyGroup("Messengers", false)]
        public int MessengerTravelTime { get; set; } = 3;

    }
}
