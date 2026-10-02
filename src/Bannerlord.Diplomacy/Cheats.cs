using Diplomacy.Actions;
using Diplomacy.CivilWar;
using Diplomacy.CivilWar.Actions;
using Diplomacy.DiplomaticAction;
using Diplomacy.DiplomaticAction.NonAggressionPact;
using Diplomacy.Extensions;
using Diplomacy.WarExhaustion;

using JetBrains.Annotations;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;

namespace Diplomacy
{
    internal sealed class CampaignCheatsExtension
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("form_alliance", "diplomacy")]
        [UsedImplicitly]
        private static string FormAlliance(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            if (!CampaignCheats.CheckParameters(strings, 2) || CampaignCheats.CheckHelp(strings))
                return "Format uses 2 kingdom ID parameters without spaces: diplomacy.form_alliance [Kingdom1] [Kingdom2]";

            var b1 = strings[0].ToLower();
            var b2 = strings[1].ToLower();

            if (b1 == b2)
                return "Cannot ally a kingdom to itself!";

            Kingdom? kingdom1 = null;
            Kingdom? kingdom2 = null;

            foreach (var k in KingdomExtensions.AllActiveKingdoms)
            {
                var id = k.Name.ToString().ToLower().Replace(" ", "");

                if (id == b1)
                    kingdom1 = k;
                else if (id == b2)
                    kingdom2 = k;
            }

            if (kingdom1 is null && kingdom2 is null)
                return "Could not find either required kingdom!";

            if (kingdom1 is null)
                return "1st kingdom ID not found: " + b1;

            if (kingdom2 is null)
                return "2nd kingdom ID not found: " + b2;

            Campaign.Current.GetCampaignBehavior<IAllianceCampaignBehavior>().StartAlliance(kingdom1, kingdom2);
            return $"Alliance formed between {kingdom1.Name} and {kingdom2.Name}!";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("form_non_aggression_pact", "diplomacy")]
        [UsedImplicitly]
        private static string FormNonAggressionPact(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            if (!CampaignCheats.CheckParameters(strings, 2) || CampaignCheats.CheckHelp(strings))
                return "Format uses 2 kingdom ID parameters without spaces: diplomacy.form_non_aggression_pact [Kingdom1] [Kingdom2]";

            var b1 = strings[0].ToLower();
            var b2 = strings[1].ToLower();

            if (b1 == b2)
                return "Cannot make a pact with itself!";

            Kingdom? kingdom1 = null;
            Kingdom? kingdom2 = null;

            foreach (var k in KingdomExtensions.AllActiveKingdoms)
            {
                var id = k.Name.ToString().ToLower().Replace(" ", "");

                if (id == b1)
                    kingdom1 = k;
                else if (id == b2)
                    kingdom2 = k;
            }

            if (kingdom1 is null && kingdom2 is null)
                return "Could not find either required kingdom!";

            if (kingdom1 is null)
                return "1st kingdom ID not found: " + b1;

            if (kingdom2 is null)
                return "2nd kingdom ID not found: " + b2;

            if (!DiplomaticAgreementManager.HasNonAggressionPact(kingdom1, kingdom2, out var pactAgreement))
            {
                FormNonAggressionPactAction.Apply(kingdom1, kingdom2, bypassCosts: true);
                return $"Non-aggression pact formed between {kingdom1.Name} and {kingdom2.Name}!";
            }
            else
            {
                return "Specified kingdoms already have a non-aggression pact!";
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("break_non_aggression_pact", "diplomacy")]
        [UsedImplicitly]
        private static string BreakNonAggressionPact(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            if (!CampaignCheats.CheckParameters(strings, 2) || CampaignCheats.CheckHelp(strings))
                return "Format uses 2 kingdom ID parameters without spaces: diplomacy.form_non_aggression_pact [Kingdom1] [Kingdom2]";

            var b1 = strings[0].ToLower();
            var b2 = strings[1].ToLower();

            if (b1 == b2)
                return "Cannot break a pact with itself!";

            Kingdom? kingdom1 = null;
            Kingdom? kingdom2 = null;

            foreach (var k in KingdomExtensions.AllActiveKingdoms)
            {
                var id = k.Name.ToString().ToLower().Replace(" ", "");

                if (id == b1)
                    kingdom1 = k;
                else if (id == b2)
                    kingdom2 = k;
            }

            if (kingdom1 is null && kingdom2 is null)
                return "Could not find either required kingdom!";

            if (kingdom1 is null)
                return "1st kingdom ID not found: " + b1;

            if (kingdom2 is null)
                return "2nd kingdom ID not found: " + b2;

            if (DiplomaticAgreementManager.HasNonAggressionPact(kingdom1, kingdom2, out var pactAgreement))
            {
                pactAgreement!.Expire();
                return $"Non-aggression pact broken between {kingdom1.Name} and {kingdom2.Name}!";
            }
            else
            {
                return "Specified kingdoms don't have a non-aggression pact!";
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_war_exhaustion", "diplomacy")]
        [UsedImplicitly]
        private static string SetWarExhaustion(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            if (!Settings.Instance!.EnableWarExhaustion)
                return "War exhaustion is disabled!";

            var isNumeric = int.TryParse(strings[2], out var targetWarExhaustion);

            if (!CampaignCheats.CheckParameters(strings, 3) || CampaignCheats.CheckHelp(strings) || !isNumeric)
                return "Format uses 2 kingdom ID parameters without spaces and a war exhaustion integer: diplomacy.set_war_exhaustion [Kingdom1] [Kingdom2] [int]";

            var b1 = strings[0].ToLower();
            var b2 = strings[1].ToLower();

            if (b1 == b2)
                return "Cannot have war exhaustion with itself!";

            Kingdom? kingdom1 = null;
            Kingdom? kingdom2 = null;

            foreach (var k in KingdomExtensions.AllActiveKingdoms)
            {
                var id = k.Name.ToString().ToLower().Replace(" ", "");

                if (id == b1)
                    kingdom1 = k;
                else if (id == b2)
                    kingdom2 = k;
            }

            if (kingdom1 is null && kingdom2 is null)
                return "Could not find either required kingdom!";

            if (kingdom1 is null)
                return "1st kingdom ID not found: " + b1;

            if (kingdom2 is null)
                return "2nd kingdom ID not found: " + b2;

            WarExhaustionManager.Instance!.AddDivineWarExhaustion(kingdom1, kingdom2, targetWarExhaustion);
            return "done!";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("change_kingdom_banner_color", "diplomacy")]
        [UsedImplicitly]
        public static string ChangeKingdomBannerColor(List<string> strings)
        {
            if (Campaign.Current == null)
                return "Campaign was not started.";

            if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
                return "Format uses 1 kingdom ID parameters without spaces: diplomacy.change_kingdom_banner_color [Kingdom1]";

            var b1 = strings[0].ToLower();

            Kingdom? kingdom1 = null;

            foreach (var k in KingdomExtensions.AllActiveKingdoms)
            {
                var id = k.Name.ToString().ToLower().Replace(" ", "");

                if (id == b1)
                    kingdom1 = k;
            }

            if (kingdom1 is null)
                return "Could not find required kingdom!";

            ChangeKingdomBannerAction.Apply(kingdom1, kingdom1.IsRebelKingdom());
            return $"Banner color changed for {kingdom1.Name}!";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("legitimize_rebel_kingdom", "diplomacy")]
        [UsedImplicitly]
        public static string LegitimizeRebelKingdom(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
                return "Format uses 1 kingdom ID parameters without spaces: diplomacy.legitimize_rebel_kingdom [Kingdom1]";

            var b1 = strings[0].ToLower();

            Kingdom? kingdom1 = null;

            foreach (var k in KingdomExtensions.AllActiveKingdoms)
            {
                var id = k.Name.ToString().ToLower().Replace(" ", "");

                if (id == b1)
                    kingdom1 = k;
            }

            if (kingdom1 is null)
                return "Could not find required kingdom!";

            if (!kingdom1.IsRebelKingdom())
                return "Specified kingdom is not a rebel kingdom!";

            foreach (var kvp in RebelFactionManager.Instance!.RebelFactions)
                if (kvp.Value != null && kvp.Value.Any(f => f.RebelKingdom == kingdom1))
                    RebelFactionManager.Instance!.RebelFactions[kvp.Key] = kvp.Value.Where(f => f.RebelKingdom != kingdom1).ToList();

            if (RebelFactionManager.Instance!.DeadRebelKingdoms.Contains(kingdom1))
                RebelFactionManager.Instance!.DeadRebelKingdoms.Remove(kingdom1);

            return $"{kingdom1.Name} is no longer a rebel kingdom!";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("seize_fief", "diplomacy")]
        [UsedImplicitly]
        private static string SeizeFief(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            if (CampaignCheats.CheckHelp(strings) || strings.Count is < 1 or > 2)
                return "Takes a settlement name without spaces and optionally how many days ago to pretend it "
                     + "was taken (default is a full year, so that it counts as long held): "
                     + "diplomacy.seize_fief [Settlement] [DaysAgo]";

            var settlement = FindFief(strings[0]);
            if (settlement is null)
                return "Settlement not found: " + strings[0];

            if (settlement.OwnerClan == Clan.PlayerClan)
                return $"{settlement.Name} already belongs to your clan.";

            var ownerClan = settlement.OwnerClan;
            if (ownerClan?.Kingdom is not { } previousKingdom)
                return $"{settlement.Name} does not belong to a kingdom, so there is nobody to return it to.";

            var daysAgo = ParseDaysAgo(strings);
            var previousOwner = ownerClan.Leader;

            ChangeOwnerOfSettlementAction.ApplyBySiege(Hero.MainHero, Hero.MainHero, settlement);

            if (!RecordSeizure(settlement, previousOwner, previousKingdom, daysAgo))
                return "Fief provenance is not being tracked in this campaign.";

            return $"{settlement.Name} taken from {previousKingdom.Name}, recorded as held for {daysAgo:0} days. "
                 + $"Return it with: diplomacy.return_fief {strings[0]} {previousKingdom.Name.ToString().Replace(" ", "")}";
        }

        /// <summary>The optional second argument of seize_fief, defaulting to a full year so the fief counts as long held.</summary>
        private static float ParseDaysAgo(List<string> strings) =>
            strings.Count == 2 && float.TryParse(strings[1], out var parsed) ? parsed : CampaignTime.DaysInYear;

        /// <summary>Makes sure a seized fief has a conquest record, then backdates it. False if provenance isn't tracked.</summary>
        private static bool RecordSeizure(Settlement settlement, Hero? previousOwner, Kingdom previousKingdom, float daysAgo)
        {
            var manager = FiefProvenanceManager.Instance;
            if (manager is null)
                return false;

            // The siege should have been recorded through FiefProvenanceBehavior; if some other
            // path was taken, record it directly so the cheat still leaves something testable.
            if (!manager.WasConqueredFrom(settlement, previousKingdom, out _))
                manager.RegisterTransfer(settlement, previousOwner, Hero.MainHero, true);

            manager.BackdateLastTransfer(settlement, daysAgo);
            return true;
        }

        /// <summary>A town or castle by its name with the spaces left out, as cheat arguments can't contain spaces.</summary>
        private static Settlement? FindFief(string name) =>
            Settlement.All.FirstOrDefault(s => (s.IsTown || s.IsCastle)
                                               && s.Name.ToString().ToLower().Replace(" ", "") == name.ToLower());

        private static Kingdom? FindKingdom(string name) =>
            KingdomExtensions.AllActiveKingdoms.FirstOrDefault(k => k.Name.ToString().ToLower().Replace(" ", "") == name.ToLower());

        private static float GetExpansionism(Kingdom? kingdom) => kingdom?.GetExpansionism() ?? 0f;

        [CommandLineFunctionality.CommandLineArgumentFunction("return_fief", "diplomacy")]
        [UsedImplicitly]
        private static string ReturnFief(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            if (CampaignCheats.CheckHelp(strings) || !CampaignCheats.CheckParameters(strings, 2))
                return "Format uses a settlement name and a kingdom name without spaces: diplomacy.return_fief [Settlement] [Kingdom]";

            var settlement = FindFief(strings[0]);
            if (settlement is null)
                return "Settlement not found: " + strings[0];

            var kingdom = FindKingdom(strings[1]);
            if (kingdom is null)
                return "Kingdom not found: " + strings[1];

            if (!ReturnFiefAction.CanReturnFief(settlement, kingdom, out var reason))
                return reason ?? "That fief cannot be returned.";

            var playerKingdom = Clan.PlayerClan.Kingdom;
            var expansionismBefore = GetExpansionism(playerKingdom);
            var relationGain = ReturnFiefAction.PreviewRelationChange(settlement, kingdom);

            ReturnFiefAction.Apply(settlement, kingdom);

            var expansionismAfter = GetExpansionism(playerKingdom);

            return $"{settlement.Name} returned to {settlement.OwnerClan?.Name.ToString() ?? kingdom.Name.ToString()} of {kingdom.Name}. "
                 + $"Relation {relationGain:+#;-#;0}. Expansionism {expansionismBefore:0.#} -> {expansionismAfter:0.#}.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("toggle_debug_mode", "diplomacy")]
        [UsedImplicitly]
        public static string ToggleUIDebugMode(List<string> strings)
        {
            UIConfig.DebugModeEnabled = !UIConfig.DebugModeEnabled;
            return "UI Debug Mode " + (UIConfig.DebugModeEnabled ? "Enabled" : "Disabled");
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("reload_ui", "diplomacy")]
        [UsedImplicitly]
        public static string ReloadUI(List<string> strings)
        {
            UIResourceManager.Update();
            return "Reloaded";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("fief_provenance", "diplomacy")]
        [UsedImplicitly]
        private static string FiefProvenance(List<string> strings)
        {
            if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
                return CampaignCheats.ErrorType;

            if (CampaignCheats.CheckHelp(strings) || !CampaignCheats.CheckParameters(strings, 1))
                return "Format uses 1 settlement name parameter without spaces: diplomacy.fief_provenance [Settlement]";

            var settlement = FindFief(strings[0]);
            if (settlement is null)
                return "Settlement not found: " + strings[0];

            var manager = FiefProvenanceManager.Instance;
            if (manager is null)
                return "Fief provenance is not being tracked in this campaign.";

            var history = manager.GetHistory(settlement);
            if (history.Count == 0)
                return $"{settlement.Name} has not changed hands since this campaign was started.";

            var currentKingdom = settlement.OwnerClan?.Kingdom?.Name.ToString() ?? "nobody";
            var report = new List<string>
            {
                $"{settlement.Name} has passed between kingdoms {manager.GetTimesChangedHands(settlement)} time(s). Held by {currentKingdom} since {manager.GetHeldByCurrentKingdomSince(settlement)}."
            };

            report.AddRange(history.Select(DescribeTransfer));

            return string.Join("\n", report);
        }

        private static string DescribeTransfer(FiefOwnershipRecord record)
        {
            var from = record.PreviousKingdom?.Name.ToString() ?? record.PreviousOwnerClan?.Name.ToString() ?? "nobody";
            var to = record.NewKingdom?.Name.ToString() ?? record.NewOwnerClan?.Name.ToString() ?? "nobody";
            return $"  {record.TransferDate}: {from} -> {to}{(record.ByConquest ? " (by siege)" : string.Empty)}";
        }
    }
}
