using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

using TaleWorlds.Localization;

namespace Diplomacy
{
    class Settings : AttributeGlobalSettings<Settings>
    {
        private const string HeadingKingdomDiplomacy = "{=sBw5Qzq3}Kingdom Diplomacy";
        private const string Costs = "{=ldLFTs92}Costs";
        private const string HeadingGoldCosts = Costs + "/" + "{=Ckd1Lsoa}Gold Costs";
        private const string HeadingInfluenceCosts = Costs + "/" + "{=SEViwYTl}Influence Costs";
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

        private bool _enableWarExhaustionCampaignMapWidget = true;

        public override string Id => "DiplomacySettings_v1.2";
        public override string DisplayName => new TextObject("{=MYz8nKqq}Diplomacy").ToString();
        public override string FolderName => "Diplomacy";
        public override string FormatType => "json2";

        // Kingdom Diplomacy
        [SettingPropertyBool(displayName: "{=tis8Ddzn}Allow Player To Claim Player-Taken Settlements", Order = 0, RequireRestart = true, HintText = "{=TfxLCxcD}Gives the player the option to claim a settlement that they have taken rather than let it go to an election.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public bool EnableFiefFirstRight { get; set; } = true;

#if v100 || v101 || v102 || v103 || v110 || v111 || v112 || v113 || v114 || v115 || v116
        [SettingPropertyBool("{=8VKC3jtN}Enable Fiefless Kingdom Elimination", Order = 10, RequireRestart = false, HintText = "{=TlymwwPZ}If enabled, kingdoms without any fiefs are destroyed when they sign a peace treaty ending the last ongoing war they participate in. Default value is enabled.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public bool EnableKingdomElimination { get; set; } = true;
#else
        [SettingPropertyBool("{=w8Hi9jJf}Delay Fiefless Kingdom Elimination", Order = 10, RequireRestart = false, HintText = "{=GDctI4Kd}If enabled, kingdoms without any fiefs will only be destroyed when they sign a peace treaty ending the last ongoing war they are involved in, not immediately after they lose their last fief. Default value is enabled.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public bool EnableKingdomElimination { get; set; } = true;
#endif

        [SettingPropertyInteger("{=ZRlNvsev}Minimum War Duration in Days", 0, 500, Order = 20, RequireRestart = false, HintText = "{=vuFT5ns8}The minimum duration (in days) that a war can last before proposing peace. Default value is 21 (quarter of a standard game year).")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int MinimumWarDurationInDays { get; set; } = 21;

        [SettingPropertyInteger("{=4MzQHMVj}Declare War Cooldown in Days", 0, 500, Order = 21, RequireRestart = false, HintText = "{=q2duqN8d}The minimum duration (in days) before re-declaring war on the same kingdom after making peace. Default value is 21 (quarter of a standard game year).")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int DeclareWarCooldownInDays { get; set; } = 21;

        [SettingPropertyBool("{=2XC8QHkl}Enable Alliances", Order = 30, RequireRestart = false, HintText = "{=5YJBZx28}If disabled, this disables the ability to form alliances for both player and AI kingdoms. Default value is enabled.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public bool EnableAlliances { get; set; } = true;

        [SettingPropertyInteger("{=5a829TiT}Alliance Tendency", -100, 100, Order = 31, RequireRestart = false, HintText = "{=7nSjs8UL}Score modifier affecting the tendency of kingdoms to form alliances. Increasing the modifier makes alliances more desirable to AI kingdoms. Default value is 0.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int AllianceTendency { get; set; } = 0;

        [SettingPropertyInteger("{=H6XMjwpF}Minimum Alliance Duration in Days", 0, 500, Order = 32, RequireRestart = false, HintText = "{=RrsWhIWi}The minimum duration (in days) that an alliance will last before it can be broken. Default value is 42 (half of a standard game year).")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int MinimumAllianceDuration { get; set; } = 42;

        [SettingPropertyInteger("{=V35hUfcc}Non-Aggression Pact Duration in Days", 0, 1000, Order = 50, RequireRestart = false, HintText = "{=KXLGZEPh}The duration (in days) that a non-aggression pact will last. Default value is 84 (one standard game year).")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int NonAggressionPactDuration { get; set; } = 84;

        [SettingPropertyInteger("{=G8BhBnRG}Non-Aggression Pact Tendency", -100, 100, Order = 51, RequireRestart = false, HintText = "{=907ER5u9}Score modifier affecting the tendency of kingdoms to form non-aggression pacts. Increasing the modifier makes non-aggression pacts more desirable to AI kingdoms. Default value is 0.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public int NonAggressionPactTendency { get; set; } = 0;

        [SettingPropertyBool(displayName: "{=6m1SspFW}Enable Player Kingdom Diplomacy Control", Order = 999, RequireRestart = false, HintText = "{=N5EouSSj}Gives the player full control over declaring war and making peace in the kingdom they belong to, even if they are just a vassal and not the leader of the kingdom. Default value is disabled.")]
        [SettingPropertyGroup(HeadingKingdomDiplomacy)]
        public bool PlayerDiplomacyControl { get; set; } = false;

        // Messengers

        [SettingPropertyBool("{=nwTyegdV}Enable Messengers Accidents", Order = 0, RequireRestart = false, HintText = "{=T7yybpw3}If enabled, adds a small chance of failure for messengers. The longer the journey, the higher the chance of an accident on the road. Default value is enabled.")]
        [SettingPropertyGroup(HeadingMessengers)]
        public bool EnableMessengerAccidents { get; set; } = true;

        [SettingPropertyBool("{=ysYCYvwT}Restrict Messengers Sending", Order = 1, RequireRestart = false, HintText = "{=5O87pSL3}If enabled, you can only send a messenger to people you have met or at least likely to know of. Otherwise, you can send a messenger to a person regardless your knowledge of addressee. Default value is enabled.")]
        [SettingPropertyGroup(HeadingMessengers)]
        public bool EnableMessengerRestictions { get; set; } = true;

        [SettingPropertyInteger("{=nnXi6MmH}Messenger Travel Time in Days", 0, 10, Order = 10, RequireRestart = false, HintText = "{=zkvCGLuK}The maximum amount of time (in days) it could take the messenger to reach the addressee. Directly affects the travelling speed of the messengers. The default value is 3.")]
        [SettingPropertyGroup(HeadingMessengers)]
        public int MessengerTravelTime { get; set; } = 3;

        [SettingPropertyInteger("{=nMwWHj4h}Send Messenger Gold Cost", 0, 10000, Order = 11, RequireRestart = false, HintText = "{=ehMf7xvE}Gold cost for sending a messenger to another character. Default value is 100.")]
        [SettingPropertyGroup(HeadingMessengers)]
        public int SendMessengerGoldCost { get; set; } = 100;

        // War Exhaustion

        [SettingPropertyBool("{=lSttctYC}Enable War Exhaustion", Order = 0, RequireRestart = true, HintText = "{=Cxyn9ROT}Enables the war exhaustion mechanic. Default value is enabled.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public bool EnableWarExhaustion { get; set; } = true;

        [SettingPropertyBool("{=Yc2YUIva}Enable Diminishing Returns", Order = 1, RequireRestart = true, HintText = "{=CaxxEEdC}Enables diminishing returns for some events when calculating war exhaustion. Default value is enabled.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public bool EnableDiminishingReturns { get; set; } = true;

        [SettingPropertyBool("{=HPFINWrc}Individual War Exhaustion Rates", Order = 2, RequireRestart = false, HintText = "{=LFVkLcIp}Enables the individual war exhaustion rates for factions. If disabled, a single rate will be calculated for every war based on combined strength of the opposing factions. Default value is enabled.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public bool IndividualWarExhaustionRates { get; set; } = true;

        [SettingPropertyBool("{=gW3eVr5E}Enable Fief Repatriation", Order = 3, RequireRestart = false, HintText = "{=KEi0UykN}If enabled, kingdoms may have to return some of their conquered fiefs back to the original owner if they lose the war substantially. Default value is enabled.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public bool EnableFiefRepatriation { get; set; } = true;

        [SettingPropertyFloatingInteger("{=8TFQWL55}War Exhaustion Per Day", 0f, 5f, "0.00\\%", Order = 10, RequireRestart = false, HintText = "{=lgza5wDq}The amount of war exhaustion added per day a war is ongoing. Not affected by war exhaustion rate. Default value is 0.25%.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerDay { get; set; } = 0.25f;

        [SettingPropertyFloatingInteger("{=PAmPVWgD}Fiefless Multiplier", 0f, 100f, "0.0", Order = 11, RequireRestart = false, HintText = "{=SdjYeYM5}Multiplier for the amount of war exhaustion added per day for a faction with no fiefs compared to a faction with fiefs. Default value is 10.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float FieflessWarExhaustionMultiplier { get; set; } = 10f;

        [SettingPropertyFloatingInteger("{=s6dNpM6M}War Exhaustion Per Casualty", 0f, 0.1f, "0.000\\%", Order = 20, RequireRestart = false, HintText = "{=NcJtGeM7}The amount of war exhaustion added when a faction has a battle casualty. Default value is 0.02%.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerCasualty { get; set; } = 0.02f;

        [SettingPropertyFloatingInteger("{=kr5zAufg}War Exhaustion Per Caravan Raid", 0f, 50f, "0.00\\%", Order = 25, RequireRestart = false, HintText = "{=PinVMCUE}The amount of war exhaustion added when a faction's caravan is raided. Default value is 2.0%.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerCaravanRaid { get; set; } = 2f;

        [SettingPropertyFloatingInteger("{=qFJ23KxQ}War Exhaustion Per Hero Imprisoned", 0f, 50f, "0.00\\%", Order = 30, RequireRestart = false, HintText = "{=wctCn9uO}The base amount of war exhaustion added when a faction's noble hero is imprisoned. Affected by the hero significance for the faction. Potentially subject to diminishing returns. Default value is 1.0%.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerImprisonment { get; set; } = 1f;

        [SettingPropertyFloatingInteger("{=4vTzbsXD}War Exhaustion Per Hero Perished", 0f, 50f, "0.00\\%", Order = 35, RequireRestart = false, HintText = "{=w80tUaVd}The base amount of war exhaustion added when a faction's noble hero is killed. Affected by the hero significance for the faction. Potentially subject to diminishing returns when multiple heroes of the same clan are killed. Default value is 5.0%.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerDeath { get; set; } = 5f;

        [SettingPropertyFloatingInteger("{=eWVGwf2m}War Exhaustion Per Raid", 0f, 50f, "0.00\\%", Order = 40, RequireRestart = false, HintText = "{=ufHDJt8H}The amount of war exhaustion added when a faction's village is raided. Potentially subject to diminishing returns. Default value is 3.0%.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerRaid { get; set; } = 3f;

        [SettingPropertyFloatingInteger("{=gGIaLKHk}War Exhaustion Per Siege", 0f, 50f, "0.00\\%", Order = 45, RequireRestart = false, HintText = "{=mCEa773h}The amount of war exhaustion added when a faction loses a city. Losing a castle adds half as much. Potentially subject to diminishing returns. Default value is 10.0%.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionPerSiege { get; set; } = 10f;

        [SettingPropertyFloatingInteger("{=JmUPtZdw}War Exhaustion When Occupied", 0f, 50f, "0.00\\%", Order = 50, RequireRestart = false, HintText = "{=541jGrpb}The amount of war exhaustion added when a faction loses all of its fiefs. Not affected by war exhaustion rate. Potentially subject to diminishing returns. Default value is 15.0%.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public float WarExhaustionWhenOccupied { get; set; } = 15f;

        [SettingPropertyInteger("{=pPHLRIml}Maximum Entries In Breakdown Tooltip", 0, 100, Order = 98, RequireRestart = false, HintText = "{=YMf0mSqy}Maximum number of entries in any given breakdown tooltip in war exhaustion report. Set this to any value that is convenient for viewing. Default value is 35.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public int MaxShownBreakdownEntries { get; set; } = 35;

        [SettingPropertyBool("{=F4iFH6un}Campaign Map Widget", Order = 99, RequireRestart = false, HintText = "{=zTvbZ8Br}Enables a dedicated war exhaustion widget for each ongoing war at the top of the campaign map screen. Default value is enabled.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public bool EnableWarExhaustionCampaignMapWidget
        {
            get => _enableWarExhaustionCampaignMapWidget;
            set
            {
                if (_enableWarExhaustionCampaignMapWidget != value)
                {
                    _enableWarExhaustionCampaignMapWidget = value;
                    OnPropertyChanged(nameof(EnableWarExhaustionCampaignMapWidget));
                }
            }
        }

        [SettingPropertyBool("{=jI9NSxtz}Enable Player War Exhaustion Debug Messages", Order = 100, RequireRestart = false, HintText = "{=LYyNbQds}Enables debug messages for war exhaustion added to the player kingdom. Default value is disabled.")]
        [SettingPropertyGroup(HeadingWarExhaustion)]
        public bool EnableWarExhaustionDebugMessages { get; set; } = false;

        //Relations

        [SettingPropertyFloatingInteger("{=6l9QNMrB}Grant Fief Positive Relation Multiplier", 0f, 3f, RequireRestart = false, HintText = "{=FQaghHo7}Multiplier for the relation gain when granting fiefs. Default value is 1.")]
        [SettingPropertyGroup(HeadingRelations)]
        public float GrantFiefPositiveRelationMultiplier { get; set; } = 1.0f;

        [SettingPropertyInteger("{=9qKclHq3}Grant Fief Relation Penalty", -50, 0, RequireRestart = false, HintText = "{=fgc8Dvg0}The relation penalty assessed when granting a fief to another clan. Default value is -2.")]
        [SettingPropertyGroup(HeadingRelations)]
        public int GrantFiefRelationPenalty { get; set; } = -2;

        // Gold Costs

        [SettingPropertyBool(displayName: "{=t4hNAoD7}Enable Scaling Gold Costs", Order = 0, RequireRestart = false, HintText = "{=5MMIDE5A}If enabled, this will scale gold costs of diplomatic actions and war reparations based on your kingdom size. Otherwise, the generic multipliers of 100 for diplomatic actions and 1000 for war reparations will apply. The default value is enabled.")]
        [SettingPropertyGroup(HeadingGoldCosts)]
        public bool ScalingGoldCosts { get; set; } = true;

        [SettingPropertyFloatingInteger(displayName: "{=vD270X6H}Scaling Gold Cost Multiplier", 0, 100, Order = 10, RequireRestart = false, HintText = "{=Gd52NzBP}Multiplier for the scaling gold costs. Default value is 5.")]
        [SettingPropertyGroup(HeadingGoldCosts)]
        public float ScalingGoldCostMultiplier { get; set; } = 5;

        [SettingPropertyFloatingInteger(displayName: "{=HFtZsD6v}Scaling War Reparations Gold Cost Multiplier", 0, 1000, Order = 11, RequireRestart = false, HintText = "{=MIhbrqbr}Multiplier for the scaling of war reparations gold costs. Default value is 50.")]
        [SettingPropertyGroup(HeadingGoldCosts)]
        public float ScalingWarReparationsGoldCostMultiplier { get; set; } = 50;

        [SettingPropertyInteger(displayName: "{=Cr6a5Jap}Defeated War Reparations Gold Cost", 0, 10000, Order = 12, RequireRestart = false, HintText = "{=NH4GNKva}The base cost in gold for losing a war of attrition against another kingdom. Affected by scaling and with scaling disabled will be multiplied by a thousand. The default value is 200.")]
        [SettingPropertyGroup(HeadingGoldCosts)]
        public int DefeatedGoldCost { get; set; } = 200;

        // Influence Costs

        [SettingPropertyBool(displayName: "{=WbOKuWbQ}Enable Influence Costs", Order = 0, RequireRestart = false, HintText = "{=K2vLGalN}If disabled, this removes influence costs for war and peace declaration actions. Default value is enabled.")]
        [SettingPropertyGroup(HeadingInfluenceCosts)]
        public bool EnableInfluenceCostsForDiplomacyActions { get; set; } = true;

        [SettingPropertyBool(displayName: "{=P1g6Ht1e}Enable Scaling Influence Costs", Order = 1, RequireRestart = false, HintText = "{=xfVFBxfj}If enabled, this will scale influence costs based on your kingdom size. Otherwise, flat influence costs are used. Default value is enabled.")]
        [SettingPropertyGroup(HeadingInfluenceCosts)]
        public bool ScalingInfluenceCosts { get; set; } = true;

        [SettingPropertyFloatingInteger(displayName: "{=TvAYJv5Q}Scaling Influence Cost Multiplier", 0, 100, Order = 10, RequireRestart = false, HintText = "{=AQ5gRYN6}Multiplier for the scaling influence costs. Default value is 5.")]
        [SettingPropertyGroup(HeadingInfluenceCosts)]
        public float ScalingInfluenceCostMultiplier { get; set; } = 5.0f;

        [SettingPropertyInteger(displayName: "{=OnTeAgin}Flat Declare War Influence Cost", 0, 10000, Order = 11, RequireRestart = false, HintText = "{=O5XvybTI}Influence cost for declaring war on another kingdom. Default value is 500.")]
        [SettingPropertyGroup(HeadingInfluenceCosts)]
        public int DeclareWarInfluenceCost { get; set; } = 500;

        [SettingPropertyInteger("{=iNsXQD2q}Flat Make Peace Influence Cost", 0, 10000, Order = 12, RequireRestart = false, HintText = "{=WB5zdvdT}Influence cost for making peace with another kingdom. Default value is 500.")]
        [SettingPropertyGroup(HeadingInfluenceCosts)]
        public int MakePeaceInfluenceCost { get; set; } = 500;

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

        [SettingPropertyBool("{=lsyl0VSX}Storyline Protection", Order = -2, RequireRestart = false, HintText = "{=EVrErrTR}When enabled, prevents the player from breaking the main storyline. Disable when using mods like \"Just Let Me Play\". Default value is enabled.")]
        public bool EnableStorylineProtection { get; set; } = true;

        public bool EnableCoalitions { get; set; } = false;
        public float CoalitionChancePercentage { get; set; } = 5.0f;
        public int CriticalExpansionism { get; set; } = 100;

        // Civil Wars

        // FIXME fix localization on new settings
        [SettingPropertyFloatingInteger("{=UdadyqpN}Daily Chance To Start Rebel Faction", 0, 1, "#0%", Order = 0, RequireRestart = false, HintText = "{=yznXCi1d}The daily chance for a clan to start a rebel faction. Default value is 5%.")]
        [SettingPropertyGroup(HeadingCivilWar)]
        public float DailyChanceToStartRebelFaction { get; set; } = 0.05f;

        [SettingPropertyFloatingInteger("{=r312b3Of}Daily Chance To Join Rebel Faction", 0, 1, "#0%", Order = 1, RequireRestart = false, HintText = "{=06oPGRj0}The daily chance for a clan to join a rebel faction that they would support. Default value is 10%.")]
        [SettingPropertyGroup(HeadingCivilWar)]
        public float DailyChanceToJoinRebelFaction { get; set; } = 0.1f;

        [SettingPropertyFloatingInteger("{=lIbUuQB1}Daily Chance To Start Civil War", 0, 1, "#0%", Order = 2, RequireRestart = false, HintText = "{=zr10ZS3L}The daily chance for a faction with enough support to start a civil war. Default value is 10%.")]
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