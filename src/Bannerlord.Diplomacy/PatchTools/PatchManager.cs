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
        public static PatchManager? Instance { get; private set; }

        public static IReadOnlyList<PatchClass> PatchClasses => _PatchClasses;

        public IReadOnlyList<PatchClass.Patch> Patches => _patches;

        public Harmony Harmony { get; init; }

        public static void PatchAll(string harmonyId)
        {
            if (Instance is not null)
                throw new InvalidOperationException($"Cannot call {nameof(PatchManager)}.{nameof(PatchAll)} more than once!");

            Instance = new PatchManager(harmonyId);
        }

        private PatchManager(string harmonyId)
        {
            var log = LogFactory.Get<PatchManager>();
            log.LogDebug($"Applying unannotated Harmony patches (domain: {harmonyId})...");

            Harmony = new(harmonyId);
            _patches = _PatchClasses.SelectMany(pc => pc.Patches).ToArray();

            foreach (var patch in _patches)
            {
                log.LogDebug($"Applying: {patch}");
                patch.Apply(Harmony);
            }

            log.LogDebug($"Applied {_patches.Length} patches from {_PatchClasses.Length} patch classes.");
        }

        private readonly PatchClass.Patch[] _patches;

        // REGISTER ALL ACTIVE HARMONY PATCH CLASSES HERE:
        private static readonly PatchClass[] _PatchClasses = new PatchClass[]
        {
            new DeclareWarActionPatch(),
            new DefaultClanPoliticsModelPatch(),
            new DiplomaticBartersBehaviorPatch(),
            new EncyclopediaDataPatch(),
            new GameMenusCallbacksPatch(),
            new KingdomDecisionProposalBehaviorPatch(),
            new SupportKingdomQuestPatch(),
            new FactionManagerPatch(),
            new DefaultEncyclopediaFactionPagePatch(),
            new RebelKingdomPatches(),
            new KingdomManagementVMPatch(),
            new MBBannerEditorGauntletScreenPatch(),
            new ViewModelPatch()
            // ... Only 1 class left to convert to declarative patching.
        };
    }
}