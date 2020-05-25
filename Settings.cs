using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Settings.Base.Global;
using TaleWorlds.Localization;

namespace DiplomacyFixes
{
    class Settings : AttributeGlobalSettings<Settings>
    {
        private const string HeadingKingdomDiplomacy = "{=sBw5Qzq3}Kingdom Diplomacy";
        private const string HeadingInfluenceCostsScaling = "{=9PlT57Nl}Influence Costs/Scaling";
        private const string HeadingInfluenceCostsFlat = "{=BazjeCZw}Influence Costs/Flat";
        private const string HeadingMessengers = "{=nueOs6m9}Messengers";
        private const string HeadingInfluenceCosts = "{=SEViwYTl}Influence Costs";
        private const string HeadingWarExhaustion = "{=V542tneW}War Exhaustion";

        private const string HeadingGoldCosts = "{=Ckd1Lsoa}Gold Costs";

        public override string Id { get; } = "DiplomacyFixesSettings_1";

        public override string FolderName { get; } = "DiplomacyFixes";

        public override string DisplayName { get; } = new TextObject("{=MYz8nKqq}Diplomacy Fixes").ToString();

        [SettingPropertyBool(displayName: "{=6m1SspFW}Enable Player Kingdom Diplomacy Control", Order = 0, RequireRestart = false, HintText = "{=N5EouSSj}Gives the player total control over their kingdom's war and peace declarations.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public bool PlayerDiplomacyControl { get; set; } = true;

        [SettingPropertyBool(displayName: "{=tis8Ddzn}Allow Player To Claim Player-Taken Settlements", Order = 0, RequireRestart = true, HintText = "{=TfxLCxcD}Gives the player the option to claim a settlement that they have taken rather than let it go to an election.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public bool EnableFiefFirstRight { get; set; } = true;

        [SettingPropertyBool(displayName: "{=WbOKuWbQ}Enable Influence Costs", Order = -1, RequireRestart = false, HintText = "{=K2vLGalN}If disabled, this removes all costs for war and peace declaration actions. Default value is true.")]
        [SettingPropertyGroup(HeadingInfluenceCosts, IsMainToggle = true)]
        public bool EnableInfluenceCostsForDiplomacyActions { get; set; } = true;

        [SettingPropertyBool(displayName: "{=P1g6Ht1e}Enable Scaling Influence Cost", Order = 0, RequireRestart = false, HintText = "{=xfVFBxfj}If enabled, this will scale influence costs based on your kingdom size. Otherwise, flat influence costs are used. Default value is true.")]
        [SettingPropertyGroup(HeadingInfluenceCosts)]
        public bool ScalingInfluenceCosts { get; set; } = true;

        [SettingPropertyFloatingInteger(displayName: "{=TvAYJv5Q}Scaling Influence Cost Multiplier", 0, 100, Order = 1, RequireRestart = false, HintText = "{=AQ5gRYN6}Multiplier for the scaling influence costs. Default value is 5.")]
        [SettingPropertyGroup(HeadingInfluenceCosts, GroupOrder = 0)]
        public float ScalingInfluenceCostMultiplier { get; set; } = 5.0f;

        [SettingPropertyFloatingInteger(displayName: "{=HFtZsD6v}Scaling War Reparations Gold Cost Multiplier", 0, 10000, Order = 0, RequireRestart = false, HintText = "{=MIhbrqbr}Multiplier for the scaling of war reparations gold costs. Default value is 100.")]
        [SettingPropertyGroup(HeadingGoldCosts, GroupOrder = 1)]
        public float ScalingWarReparationsGoldCostMultiplier { get; set; } = 100.0f;

        [SettingPropertyInteger(displayName: "{=OnTeAgin}Flat Declare War Influence Cost", 0, 10000, Order = 2, RequireRestart = false, HintText = "{=O5XvybTI}Influence cost for declaring war on another kingdom. Default value is 100.")]
        [SettingPropertyGroup(HeadingInfluenceCosts)]
        public int DeclareWarInfluenceCost { get; set; } = 100;

        [SettingPropertyInteger("{=iNsXQD2q}Flat Make Peace Influence Cost", 0, 10000, Order = 3, RequireRestart = false, HintText = "{=WB5zdvdT}Influence cost for making peace with another kingdom. Default value is 100.")]
        [SettingPropertyGroup(HeadingInfluenceCosts)]
        public int MakePeaceInfluenceCost { get; set; } = 100;

        [SettingPropertyInteger("{=ZRlNvsev}Minimum War Duration in Days", 0, 500, Order = 1, RequireRestart = false, HintText = "{=vuFT5ns8}The minimum duration (in days) that a war can last before proposing peace. Default value is 10.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int MinimumWarDurationInDays { get; set; } = 10;

        [SettingPropertyInteger("{=4MzQHMVj}Declare War Cooldown in Days", 0, 500, Order = 2, RequireRestart = false, HintText = "{=q2duqN8d}The minimum duration (in days) to declare war after making peace. Default value is 10.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int DeclareWarCooldownInDays { get; set; } = 10;

        [SettingPropertyInteger("{=H6XMjwpF}Minimum Alliance Duration in Days", 0, 500, Order = 3, RequireRestart = false, HintText = "{=RrsWhIWi}The minimum duration (in days) that an alliance can last. Default value is 10.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int MinimumAllianceDuration { get; internal set; } = 10;

        [SettingPropertyInteger("{=qeDOmURl}Send Messenger Influence Cost", 0, 10000, RequireRestart = false, HintText = "{=Lkos6GQb}Influence cost for sending a messenger to another leader. Default value is 10.")]
        [SettingPropertyGroup(HeadingMessengers)]
        public int SendMessengerInfluenceCost { get; set; } = 10;

        [SettingPropertyInteger("{=nnXi6MmH}Messenger Travel Time in Days", 0, 500, RequireRestart = false, HintText = "{=zkvCGLuK}The amount of time (in days) a messenger takes to reach a kingdom's leader. Default value is 3.")]
        [SettingPropertyGroup(HeadingMessengers)]
        public int MessengerTravelTime { get; set; } = 3;

        [SettingPropertyBool("{=lSttctYC}Enable War Exhaustion", RequireRestart = true, HintText = "{=Cxyn9ROT}If disabled, this disables the war exhaustion mechanic. Default value is enabled.")]
        [SettingPropertyGroup(HeadingWarExhaustion, IsMainToggle = true)]
        public bool EnableWarExhaustion { get; set; } = true;

        [SettingPropertyFloatingInteger("{=Bh7vme9y}Max War Exhaustion", 0f, 1000f, RequireRestart = false, HintText = "{=6Ij1WEWz}The amount of war exhaustion that forces a faction to propose peace. Default value is 100.0.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float MaxWarExhaustion { get; set; } = WarExhaustionManager.DefaultMaxWarExhaustion;

        [SettingPropertyFloatingInteger("{=8TFQWL55}War Exhaustion Per Day", 0f, 5f, RequireRestart = false, HintText = "{=lgza5wDq}The amount of war exhaustion added per day a war is ongoing. Default value is 1.0.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerDay { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("{=s6dNpM6M}War Exhaustion Per Casualty", 0f, 0.1f, "0.000", RequireRestart = false, HintText = "{=NcJtGeM7}The amount of war exhaustion added when a faction has a battle casualty. Default value is 0.01.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerCasualty { get; set; } = 0.01f;

        [SettingPropertyFloatingInteger("{=gGIaLKHk}War Exhaustion Per Siege", 0f, 50f, RequireRestart = false, HintText = "{=mCEa773h}The amount of war exhaustion added when a faction loses a city. Default value is 10.0.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerSiege { get; set; } = 10f;

        [SettingPropertyFloatingInteger("{=eWVGwf2m}War Exhaustion Per Raid", 0f, 50f, RequireRestart = false, HintText = "{=ufHDJt8H}The amount of war exhaustion added when a faction's village is raided. Default value is 3.0.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerRaid { get; set; } = 3f;

        [SettingPropertyFloatingInteger("{=Fyion5Hw}War Exhaustion Decay per Day", 0f, 50f, RequireRestart = false, HintText = "{=o99AUWR8}The amount of war exhaustion decay per day the factions are at peace. Default value is 1.0.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionDecayPerDay { get; set; } = 1.0f;

        [SettingPropertyBool("{=jI9NSxtz}Enable Player War Exhaustion Debug Messages", Order = 100, RequireRestart = false, HintText = "{=LYyNbQds}Enables debug messages for war exhaustion added to the player kingdom. Default value is false.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public bool EnableWarExhaustionDebugMessages { get; internal set; } = false;
        
    }
}
