using Diplomacy.CivilWar;
using Diplomacy.CivilWar.Actions;
using Diplomacy.DiplomaticAction;
using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.DiplomaticAction.NonAggressionPact;
using Diplomacy.Extensions;
using Diplomacy.WarExhaustion;

using JetBrains.Annotations;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
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

            DeclareAllianceAction.Apply(kingdom1, kingdom2, bypassCosts: true);
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
    }
}