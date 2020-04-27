using MBOptionScreen.Attributes;
using MBOptionScreen.Attributes.v2;
using MBOptionScreen.Settings;

namespace DiplomacyFixes
{
    class Settings : AttributeSettings<Settings>
    {
        public override string Id { get; set; } = "DiplomacyFixesSettings_1";

        public override string ModuleFolderName { get; } = "DiplomacyFixes";

        public override string ModName { get; } = "Diplomacy Fixes";

        [SettingPropertyBool(displayName: "Enable Player Kingdom Diplomacy Control", Order = 0, RequireRestart = false, HintText = "Gives the player total control over their kingdom's war and peace declarations.")]
        [SettingPropertyGroup("Kingdom Diplomacy")]
        public bool PlayerDiplomacyControl { get; set; } = true;

        [SettingPropertyBool(displayName: "Enable Influence Costs", RequireRestart = false, HintText = "If disabled, this removes all costs for war and peace declaration actions. Default value is true.")]
        [SettingPropertyGroup("Influence Costs", isMainToggle: true)]
        public bool EnableInfluenceCostsForDiplomacyActions { get; set; } = true;

        [SettingPropertyBool(displayName: "Enable Scaling Influence Cost", Order = 0, RequireRestart = false, HintText = "If enabled, this will scale influence costs based on your kingdom size. Otherwise, flat influence costs are used. Default value is true.")]
        [SettingPropertyGroup("Influence Costs/Scaling", IsMainToggle = true)]
        public bool ScalingInfluenceCosts { get; set; } = true;

        [SettingPropertyFloatingInteger(displayName: "Scaling Influence Cost Multiplier", 0, 100, Order = 1, RequireRestart = false, HintText = "Multiplier for the scaling influence costs. Default value is 5.")]
        [SettingPropertyGroup("Influence Costs/Scaling")]
        public float ScalingInfluenceCostMultiplier { get; set; } = 5.0f;

        [SettingPropertyInteger(displayName: "Flat Declare War Influence Cost", 0, 100, Order = 2, RequireRestart = false, HintText = "Influence cost for declaring war on another kingdom. Default value is 100.")]
        [SettingPropertyGroup("Influence Costs/Flat")]
        public int DeclareWarInfluenceCost { get; set; } = 100;

        [SettingPropertyInteger("Flat Make Peace Influence Cost", 0, 10000, Order = 3, RequireRestart = false, HintText ="Influence cost for making peace with another kingdom. Default value is 100.")]
        [SettingPropertyGroup("Influence Costs/Flat")]
        public int MakePeaceInfluenceCost { get; set; } = 100;

        [SettingPropertyInteger("Minimum War Duration in Days", 0, 500, Order = 1, RequireRestart = false, HintText = "The minimum duration (in days) that a war can last before proposing peace. Default value is 10.")]
        [SettingPropertyGroup("Kingdom Diplomacy")]
        public int MinimumWarDurationInDays { get; set; } = 10;

        [SettingPropertyInteger("Declare War Cooldown in Days", 0, 500, Order = 2, RequireRestart = false, HintText ="The minimum duration (in days) to declare war after making peace. Default value is 10.")]
        [SettingPropertyGroup("Kingdom Diplomacy")]
        public int DeclareWarCooldownInDays { get; set; } = 10;

        [SettingPropertyInteger("Send Messenger Influence Cost", 0, 10000, RequireRestart = false, HintText = "Influence cost for sending a messenger to another leader. Default value is 20.")]
        [SettingPropertyGroup("Messengers")]
        public int SendMessengerInfluenceCost { get; set; } = 100;

        [SettingPropertyInteger("Messenger Travel Time in Days", 0, 500, RequireRestart = false, HintText = "The amount of time (in days) a messenger takes to reach a kingdom's leader. Default value is 3.")]
        [SettingPropertyGroup("Messengers")]
        public int MessengerTravelTime { get; set; } = 3;
    }
}
