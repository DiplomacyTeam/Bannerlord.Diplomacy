using MBOptionScreen.Attributes;
using MBOptionScreen.Settings;

namespace DiplomacyFixes
{
    class Settings : AttributeSettings<Settings>
    {
        public override string Id { get; set; } = "DiplomacyFixesSettings_1";

        public override string ModuleFolderName { get; } = "DiplomacyFixes";

        public override string ModName { get; } = "Diplomacy Fixes";

        [SettingProperty("Disable all influence costs.", false, "If disabled, this removes all costs for diplomacy actions. Default value is true.")]
        [SettingPropertyGroup("Kingdom Diplomacy Settings", true)]
        public bool EnableInfluenceCostsForDiplomacyActions { get; set; } = true;

        [SettingProperty("Enable Scaling Influence Cost", false, "If enabled, this will scale influence costs based on your kingdom size. Default value is true.")]
        [SettingPropertyGroup("Kingdom Diplomacy Settings", false)]
        public bool ScalingInfluenceCosts { get; set; } = true;

        [SettingProperty("Scaling Influence Cost Multiplier", 0f, 100f, false, "Multiplier for the scaling influence costs. Default value is 5.")]
        [SettingPropertyGroup("Kingdom Diplomacy Settings", false)]
        public float ScalingInfluenceCostMultiplier { get; set; } = 5.0f;

        [SettingProperty("Flat Declare War Influence Cost", 0, 10000, false, "Influence cost for declaring war on another kingdom. Default value is 100.")]
        [SettingPropertyGroup("Kingdom Diplomacy Settings", false)]
        public int DeclareWarInfluenceCost { get; set; } = 100;

        [SettingProperty("Flat Make Peace Influence Cost", 0, 10000, false, "Influence cost for making peace with another kingdom. Default value is 100.")]
        [SettingPropertyGroup("Kingdom Diplomacy Settings", false)]

        public int MakePeaceInfluenceCost { get; set; } = 100;
    }
}
