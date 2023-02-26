using Diplomacy.Patches;

using HarmonyLib;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Diplomacy.PatchTools
{
    /// <summary>
    /// Upon first and only instantiation, collects all of the active unannotated Harmony patches in its assembly and wires them.
    /// </summary>
    /// <remarks>
    /// Rather than discover Harmony patch classes via reflection, they are instantiated directly in the <see cref="PatchManager"/>.
    /// This is in order to establish a clear code reachability path to the patches in the face of an optimizing compiler/runtime.
    /// Subject to change if paranoia levels drop regarding hard-to-detect do-nothing patches thanks to the optimizer.
    /// </remarks>
    internal sealed class PatchManager
    {
        public static PatchManager? MainInstance { get; private set; }
        public static PatchManager? CampaignInstance { get; private set; }

        public static IReadOnlyList<PatchClass> MainPatchClasses => _mainPatchClasses;
        public static IReadOnlyList<PatchClass> CampaignPatchClasses => _campaignPatchClasses;

        public IReadOnlyList<PatchClass.Patch> Patches => _patches;

        public Harmony Harmony { get; init; }

        public static void ApplyMainPatches(string harmonyId)
        {
            if (MainInstance is not null)
                throw new InvalidOperationException($"Cannot call {nameof(PatchManager)}.{nameof(ApplyMainPatches)} more than once!");

            MainInstance = new PatchManager(harmonyId);
        }

        public static void ApplyCampaignPatches(string harmonyId)
        {
            CampaignInstance ??= new PatchManager(harmonyId, false);
        }

        public static void RemoveCampaignPatches()
        {
            if (CampaignInstance is null)
                throw new InvalidOperationException($"Cannot call {nameof(PatchManager)}.{nameof(RemoveCampaignPatches)} before calling {nameof(PatchManager)}.{nameof(ApplyCampaignPatches)}!");

            var log = LogFactory.Get<PatchManager>();
            log.LogDebug($"Removing unannotated Harmony patches (domain: {CampaignInstance.Harmony.Id})...");

            foreach (var patch in CampaignInstance.Patches)
            {
                log.LogDebug($"Removing: {patch}");
                patch.Remove(CampaignInstance.Harmony);
            }

            CampaignInstance = null;
        }

        private PatchManager(string harmonyId, bool useMainPatches = true)
        {
            var log = LogFactory.Get<PatchManager>();
            log.LogDebug($"Applying unannotated Harmony patches (domain: {harmonyId})...");

            Harmony = new(harmonyId);
            var sourcePatches = useMainPatches ? _mainPatchClasses : _campaignPatchClasses;
            _patches = sourcePatches.SelectMany(pc => pc.Patches).ToArray();

            foreach (var patch in _patches)
            {
                log.LogDebug($"Applying: {patch}");
                patch.Apply(Harmony);
            }

            log.LogDebug($"Applied {_patches.Length} patches of {sourcePatches.Length} patch classes (domain: {harmonyId})..");
        }

        private readonly PatchClass.Patch[] _patches;

        // REGISTER ALL ACTIVE HARMONY PATCH CLASSES TO USE OnSubModuleLoad HERE:
        private static readonly PatchClass[] _mainPatchClasses = new PatchClass[]
        {
#if v100 || v101 || v102 || v103
            new DeclareWarActionPatch(),
#endif
            new DefaultClanPoliticsModelPatch(),
            new DiplomaticBartersBehaviorPatch(),
            new GameMenusCallbacksPatch(),
            new KingdomDecisionProposalBehaviorPatch(),
            new SupportKingdomQuestPatch(),
            new FactionManagerPatch(),
            new DefaultEncyclopediaFactionPagePatch(),
            new KingdomManagementVMPatch(),
            new MBBannerEditorGauntletScreenPatch(),
            new MakePeaceKingdomDecisionPatch()
        };

        // REGISTER ALL ACTIVE HARMONY PATCH CLASSES TO USE OnGameStart HERE:
        private static readonly PatchClass[] _campaignPatchClasses = new PatchClass[]
        {
            new RebelKingdomPatches(),
        };
    }
}