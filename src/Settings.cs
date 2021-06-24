using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Settings.Base.Global;
using TaleWorlds.Localization;

namespace Diplomacy
{
    class Settings : AttributeGlobalSettings<Settings>
    {
        private const string HeadingKingdomDiplomacy = "{=sBw5Qzq3}Kingdom Diplomacy";
        private const string HeadingInfluenceCosts = "{=SEViwYTl}Influence Costs";
        private const string HeadingGoldCosts = "{=Ckd1Lsoa}Gold Costs";
        private const string HeadingWarExhaustion = "{=V542tneW}War Exhaustion";
        private const string HeadingInfluenceBalancing = "{=8ZPKToTq}Influence Balancing";
        private const string HeadingInfluenceDecay = HeadingInfluenceBalancing + "/" + "{=vzKRX2JA}Influence Decay";
        private const string HeadingCorruption = HeadingInfluenceBalancing + "/" + "{=e5fHDXgl}Corruption";
        private const string HeadingRelations = "{=OfEMJWiR}Relation";
        private const string HeadingMessengers = "{=nueOs6m9}Messengers";
        private const string HeadingExpansionism = "{=ZqQaUIil}Expansionism";
        private const string HeadingCoalitions = "{=2raR1ZHv}Coalitions";
        private const string HeadingInfluenceCostsScaling = "{=9PlT57Nl}Influence Costs/Scaling";
        private const string HeadingInfluenceCostsFlat = "{=BazjeCZw}Influence Costs/Flat";
        private const string HeadingUnfinishedFeatures = "{=f6n2UAEC}Unfinished Features";

        private const string HeadingCivilWar = "{=eDZeFUTH}Civil Wars";

        public override string Id => "DiplomacySettings_1";
        public override string DisplayName => new TextObject("{=MYz8nKqq}Diplomacy").ToString();
        public override string FolderName => "Diplomacy";
        public override string FormatType => "json2";

        // Kingdom Diplomacy


        [SettingPropertyBool(displayName: "{=tis8Ddzn}Allow Player To Claim Player-Taken Settlements", Order = 0, RequireRestart = true, HintText = "{=TfxLCxcD}Gives the player the option to claim a settlement that they have taken rather than let it go to an election.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public bool EnableFiefFirstRight { get; set; } = true;

        [SettingPropertyInteger("{=ZRlNvsev}Minimum War Duration in Days", 0, 500, Order = 1, RequireRestart = false, HintText = "{=vuFT5ns8}The minimum duration (in days) that a war can last before proposing peace. Default value is 10.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int MinimumWarDurationInDays { get; set; } = 10;

        [SettingPropertyInteger("{=4MzQHMVj}Declare War Cooldown in Days", 0, 500, Order = 2, RequireRestart = false, HintText = "{=q2duqN8d}The minimum duration (in days) to declare war after making peace. Default value is 100.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int DeclareWarCooldownInDays { get; set; } = 100;

        [SettingPropertyBool("{=2XC8QHkl}Enable Alliances", Order = 3, RequireRestart = false, HintText = "{=5YJBZx28}If disabled, this disables the ability to form alliances for both player and AI kingdoms. Default value is enabled.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public bool EnableAlliances { get; set; } = true;

        [SettingPropertyInteger("{=H6XMjwpF}Minimum Alliance Duration in Days", 0, 500, Order = 4, RequireRestart = false, HintText = "{=RrsWhIWi}The minimum duration (in days) that an alliance can last. Default value is 10.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int MinimumAllianceDuration { get; set; } = 10;

        [SettingPropertyInteger("{=V35hUfcc}Non-Aggression Pact Duration in Days", 0, 1000, Order = 5, RequireRestart = false, HintText = "{=KXLGZEPh}The duration (in days) that a non-aggression pact will last. Default value is 100.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int NonAggressionPactDuration { get; set; } = 100;

        [SettingPropertyInteger("{=G8BhBnRG}Non-Aggression Pact Tendency", -100, 100, Order = 6, RequireRestart = false, HintText = "{=907ER5u9}Score modifier affecting the tendency of kingdoms to form non-aggression pacts. Increasing the modifier makes non-aggression pacts more desirable to AI kingdoms. Default value is 0.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int NonAggressionPactTendency { get; set; } = 0;

        [SettingPropertyInteger("{=5a829TiT}Alliance Tendency", -100, 100, Order = 7, RequireRestart = false, HintText = "{=7nSjs8UL}Score modifier affecting the tendency of kingdoms to form alliances. Increasing the modifier makes alliances more desirable to AI kingdoms. Default value is 0.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int AllianceTendency { get; set; } = 0;

        [SettingPropertyBool(displayName: "{=6m1SspFW}Enable Player Kingdom Diplomacy Control", Order = 999, RequireRestart = false, HintText = "{=N5EouSSj}Gives the player total control over their kingdom's war and peace declarations. Default value is false.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public bool PlayerDiplomacyControl { get; set; } = false;

        // Messengers

        [SettingPropertyInteger("{=nMwWHj4h}Send Messenger Gold Cost", 0, 10000, RequireRestart = false, HintText = "{=ehMf7xvE}Gold cost for sending a messenger to another character. Default value is 100.")]
        [SettingPropertyGroup(HeadingMessengers)]
        public int SendMessengerGoldCost { get; set; } = 100;

        [SettingPropertyInteger("{=nnXi6MmH}Messenger Travel Time in Days", 0, 500, RequireRestart = false, HintText = "{=zkvCGLuK}The amount of time (in days) a messenger takes to reach a kingdom's leader. Default value is 3.")]
        [SettingPropertyGroup(HeadingMessengers)]
        public int MessengerTravelTime { get; set; } = 3;

        // War Exhaustion

        [SettingPropertyBool("{=lSttctYC}Enable War Exhaustion", RequireRestart = true, HintText = "{=Cxyn9ROT}If disabled, this disables the war exhaustion mechanic. Default value is enabled.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public bool EnableWarExhaustion { get; set; } = true;

        [SettingPropertyFloatingInteger("{=8TFQWL55}War Exhaustion Per Day", 0f, 5f, RequireRestart = false, HintText = "{=lgza5wDq}The amount of war exhaustion added per day a war is ongoing. Default value is 1.0.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerDay { get; set; } = 0.5f;

        [SettingPropertyFloatingInteger("{=s6dNpM6M}War Exhaustion Per Casualty", 0f, 0.1f, "0.000", RequireRestart = false, HintText = "{=NcJtGeM7}The amount of war exhaustion added when a faction has a battle casualty. Default value is 0.01.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerCasualty { get; set; } = 0.01f;

        [SettingPropertyFloatingInteger("{=gGIaLKHk}War Exhaustion Per Siege", 0f, 50f, RequireRestart = false, HintText = "{=mCEa773h}The amount of war exhaustion added when a faction loses a city. Default value is 10.0.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerSiege { get; set; } = 10f;

        [SettingPropertyFloatingInteger("{=eWVGwf2m}War Exhaustion Per Raid", 0f, 50f, RequireRestart = false, HintText = "{=ufHDJt8H}The amount of war exhaustion added when a faction's village is raided. Default value is 3.0.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerRaid { get; set; } = 3f;

        [SettingPropertyFloatingInteger("{=Fyion5Hw}War Exhaustion Decay per Day", 0f, 50f, RequireRestart = false, HintText = "{=o99AUWR8}The amount of war exhaustion decay per day the factions are at peace. Default value is 2.0.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionDecayPerDay { get; set; } = 2.0f;

        [SettingPropertyBool("{=jI9NSxtz}Enable Player War Exhaustion Debug Messages", Order = 100, RequireRestart = false, HintText = "{=LYyNbQds}Enables debug messages for war exhaustion added to the player kingdom. Default value is false.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public bool EnableWarExhaustionDebugMessages { get; set; } = false;

        //Relations

        [SettingPropertyFloatingInteger("{=6l9QNMrB}Grant Fief Positive Relation Multiplier", 0f, 3f, RequireRestart = false, HintText = "{=FQaghHo7}Multiplier for the relation gain when granting fiefs. Default value is 1.")]
        [SettingPropertyGroup(HeadingRelations)]
        public float GrantFiefPositiveRelationMultiplier { get; set; } = 1.0f;

        [SettingPropertyInteger("{=9qKclHq3}Grant Fief Relation Penalty", -50, 0, RequireRestart = false, HintText = "{=fgc8Dvg0}The relation penalty assessed when granting a fief to another clan. Default value is -2.")]
        [SettingPropertyGroup(HeadingRelations)]
        public int GrantFiefRelationPenalty { get; set; } = -2;

        // Costs
        
        [SettingPropertyFloatingInteger(displayName: "{=HFtZsD6v}Scaling War Reparations Gold Cost Multiplier", 0, 10000, Order = 0, RequireRestart = false, HintText = "{=MIhbrqbr}Multiplier for the scaling of war reparations gold costs. Default value is 100.")]
        [SettingPropertyGroup(HeadingGoldCosts)]
        public float ScalingWarReparationsGoldCostMultiplier { get; set; } = 100.0f;

        [SettingPropertyInteger(displayName: "{=OnTeAgin}Flat Declare War Influence Cost", 0, 10000, Order = 2, RequireRestart = false, HintText = "{=O5XvybTI}Influence cost for declaring war on another kingdom. Default value is 100.")]
        [SettingPropertyGroup(HeadingInfluenceCosts)]
        public int DeclareWarInfluenceCost { get; set; } = 100;

        [SettingPropertyInteger("{=iNsXQD2q}Flat Make Peace Influence Cost", 0, 10000, Order = 3, RequireRestart = false, HintText = "{=WB5zdvdT}Influence cost for making peace with another kingdom. Default value is 100.")]
        [SettingPropertyGroup(HeadingInfluenceCosts)]
        public int MakePeaceInfluenceCost { get; set; } = 100;

        [SettingPropertyBool(displayName: "{=WbOKuWbQ}Enable Influence Costs", RequireRestart = false, HintText = "{=K2vLGalN}If disabled, this removes all costs for war and peace declaration actions. Default value is true.")]
        [SettingPropertyGroup(HeadingInfluenceCosts)]
        public bool EnableInfluenceCostsForDiplomacyActions { get; set; } = true;

        [SettingPropertyBool(displayName: "{=P1g6Ht1e}Enable Scaling Influence Cost", Order = 0, RequireRestart = false, HintText = "{=xfVFBxfj}If enabled, this will scale influence costs based on your kingdom size. Otherwise, flat influence costs are used. Default value is true.")]
        [SettingPropertyGroup(HeadingInfluenceCosts)]
        public bool ScalingInfluenceCosts { get; set; } = true;

        [SettingPropertyFloatingInteger(displayName: "{=TvAYJv5Q}Scaling Influence Cost Multiplier", 0, 100, Order = 1, RequireRestart = false, HintText = "{=AQ5gRYN6}Multiplier for the scaling influence costs. Default value is 5.")]
        [SettingPropertyGroup(HeadingInfluenceCosts)]
        public float ScalingInfluenceCostMultiplier { get; set; } = 5.0f;
        
        // Influence

        [SettingPropertyBool("{=4jlYRUdZ}Enable Influence Balancing", RequireRestart = false, HintText = "{=tOMN9DQD}Enables influence balancing. May need to be disabled for mod compatibility.")]
        [SettingPropertyGroup(HeadingInfluenceBalancing)]
        public bool EnableInfluenceBalancing { get; set; } = true;

        [SettingPropertyInteger("{=WTeM4Rba}Maximum Influence Loss per Day", 0, 1000, RequireRestart = false, HintText = "{=CGixJ6ng}The maximum amount of daily influence loss. Default value is 20.")]
        [SettingPropertyGroup(HeadingInfluenceBalancing)]
        public int MaximumInfluenceLoss { get; set; } = 20;

        [SettingPropertyBool("{=szYr4OTG}Enable Influence Decay", RequireRestart = false, HintText = "{=rKS9z4Sz}Enables influence decay, which gradually decays influence each day. Default value is enabled.")]
        [SettingPropertyGroup(HeadingInfluenceDecay)]
        public bool EnableInfluenceDecay { get; set; } = true;

        [SettingPropertyInteger("{=pVE1yNlm}Influence Decay Threshold", 0, 50000, RequireRestart = false, HintText = "{=MnLk08uW}The amount of influence exempt from influence decay. Default value is 1000.")]
        [SettingPropertyGroup(HeadingInfluenceDecay)]
        public int InfluenceDecayThreshold { get; set; } = 1000;

        [SettingPropertyFloatingInteger("{=yob5ZHtz}Influence Decay Percentage per Day", 0f, 10f, "0.000", RequireRestart = false, HintText = "{=IYFl4kJx}The percentage of influence that decays away per day. Default value is 2.0.")]
        [SettingPropertyGroup(HeadingInfluenceDecay)]
        public float InfluenceDecayPercentage { get; set; } = 2f;

        [SettingPropertyBool("{=KWAH0S4h}Enable Corruption", RequireRestart = false, HintText = "{=trgnZ0pn}Enables corruption, which gradually decays a clan's influence each day when holding too many fiefs. Default value is enabled.")]
        [SettingPropertyGroup(HeadingCorruption)]
        public bool EnableCorruption { get; set; } = true;

        // Expansionism

        [SettingPropertyInteger("{=Pr4dcRKm}Minimum Expansionism Per Fief", 0, 100, RequireRestart = false, HintText = "{=9qcAWjI7}The minimum amount of expansionism a fief contributes. Default value is 3.")]
        [SettingPropertyGroup(HeadingExpansionism)]
        public int MinimumExpansionismPerFief { get; set; } = 3;

        [SettingPropertyInteger("{=MnhMW8iq}Expansionism Per Siege", 0, 100, RequireRestart = false, HintText = "{=lCbie62E}Expanionism added per successful siege. Default value is 20.")]
        [SettingPropertyGroup(HeadingExpansionism)]
        public int ExpanisonismPerSiege { get; set; } = 20;

        [SettingPropertyInteger("{=mEXGC0h3}Expansionism Decay Per Day", 0, 100, RequireRestart = false, HintText = "{=kgPeQvqE}The amount of expansionism that decays each day. Default value is 1.")]
        [SettingPropertyGroup(HeadingExpansionism)]
        public int ExpansionismDecayPerDay { get; set; } = 1;

        // Misc

        [SettingPropertyBool("{=lsyl0VSX}Storyline Protection", Order = -2, RequireRestart = false, HintText = "{=EVrErrTR}When enabled, prevents the player from breaking the main storyline. Disable when using mods like \"Just Let Me Play\". Default value is true.")]
        public bool EnableStorylineProtection { get; set; } = true;

        [SettingPropertyBool("{=HSoZfvXH}Enable Usurp Throne Feature", RequireRestart = false, HintText = "{=ZIAo4er4}When enabled, this allows the player to usurp the throne of a kingdom. This feature is unbalanced and is pending a rework and thus not recommended. Default value is false.")]
        [SettingPropertyGroup(HeadingUnfinishedFeatures, GroupOrder = 999)]
        public bool EnableUsurpThroneFeature { get; set; } = false;

        public bool EnableCoalitions { get; set; } = false;
        public float CoalitionChancePercentage { get; set; } = 5.0f;
        public int CriticalExpansionism { get; set; } = 100;

        // Civil Wars

        // FIXME fix localization on new settings
        [SettingPropertyFloatingInteger("{=UdadyqpN}Daily Chance To Start Rebel Faction", 0, 1, "#0%", Order = 0, RequireRestart = false, HintText = "{=yznXCi1d}The daily chance for a clan to start a rebel faction. Default value is 100%.")]
        [SettingPropertyGroup(HeadingCivilWar)]
        public float DailyChanceToStartRebelFaction { get; set; } = 0.05f;

        [SettingPropertyFloatingInteger("{=r312b3Of}Daily Chance To Join Rebel Faction", 0, 1, "#0%", Order = 1, RequireRestart = false, HintText = "{=06oPGRj0}The daily chance for a clan to join a rebel faction that they would support. Default value is 100%.")]
        [SettingPropertyGroup(HeadingCivilWar)]
        public float DailyChanceToJoinRebelFaction { get; set; } = 0.1f;

        [SettingPropertyFloatingInteger("{=lIbUuQB1}Daily Chance To Start Civil War", 0, 1, "#0%", Order = 2, RequireRestart = false, HintText = "{=zr10ZS3L}The daily chance for a faction with enough support to start a civil war. Default value is 50%.")]
        [SettingPropertyGroup(HeadingCivilWar)]
        public float DailyChanceToStartCivilWar { get; set; } = 0.1f;

        [SettingPropertyInteger("{=VB97zQeJ}Minimum Time Between Civil Wars In Days", 0, 1000, Order = 3, RequireRestart = false, HintText = "{=5cUqaLk4}The minimum amount of time required between a civil war and when new rebel factions can arise in a kingdom. Default value is 240.")]
        [SettingPropertyGroup(HeadingCivilWar)]
        public int MinimumTimeSinceLastCivilWarInDays { get; set; } = 240;

        [SettingPropertyInteger("{=VNILqmoY}Maximum Faction Duration in Days", 0, 1000, Order = 4, RequireRestart = false, HintText = "{=rNJIvaSm}The maximum amount of time that a faction can last without engaging in a civil war. Default value is 120.")]
        [SettingPropertyGroup(HeadingCivilWar)]
        public int MaximumFactionDurationInDays { get; set; } = 120;

        [SettingPropertyInteger("{=6kNmdLxZ}Faction Creation Influence Cost", 0, 1000, Order = 5, RequireRestart = false, HintText = "{=EwbXg3va}The influence cost of starting a faction. Default value is 100.")]
        [SettingPropertyGroup(HeadingCivilWar)]
        public int FactionCreationInfluenceCost { get; set; } = 100;

        [SettingPropertyInteger("{=7wK0mmw1}Faction Tendency", -100, 100, Order = 5, RequireRestart = false, HintText = "{=aqvIdG7w}Score modifier affecting the tendency of clans to create or join factions. Increasing the modifier increases faction participation. Default value is 0.")]
        [SettingPropertyGroup(HeadingCivilWar)]
        public int FactionTendency { get; internal set; } = 0;

        /*
[SettingPropertyBool("{=ZIf1tRII}Enable Coalitions", RequireRestart = false, HintText = "{=8v8q0OGu}Enables coalitions, which allow factions to band together against a strong, expansionist faction. Default value is enabled.")]
[SettingPropertyGroup(HeadingCoalitions, IsMainToggle = true, GroupOrder = (int)GroupOrder.HeadingCoalitions)]
public bool EnableCoalitions { get; set; } = true;

[SettingPropertyInteger("{=1kSd88X5}Expansionism Threshold", 0, 10000, RequireRestart = false, HintText = "{=EUnASoee}The amount of expansionism a faction requires before a coalition is formed against it. Default value is 100.")]
[SettingPropertyGroup(HeadingCoalitions)]
public int CriticalExpansionism { get; set; } = 100;
[SettingPropertyFloatingInteger("{=zwbL9BHe}Coalition Chance Percentage per Day", 0f, 100f, "0.00", RequireRestart = false, HintText = "{=9ugCWA3v}The percentage change of a coalition forming against a faction over the expansionism threshold. Default value is 5.")]
[SettingPropertyGroup(HeadingCoalitions)]
public float CoalitionChancePercentage { get; set; } = 5.0f;
*/
    }
}
