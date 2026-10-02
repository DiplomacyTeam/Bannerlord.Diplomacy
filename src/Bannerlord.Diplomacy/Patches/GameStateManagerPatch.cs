using Diplomacy.Messengers;
using Diplomacy.PatchTools;

using HarmonyLib;

using System.Collections.Generic;

using TaleWorlds.Core;

namespace Diplomacy.Patches
{
    internal sealed class GameStateManagerPatch : PatchClass<GameStateManagerPatch, GameStateManager>
    {
        protected override IEnumerable<Patch> Prepare()
        {
            return new[]
            {
                new Prefix(nameof(OnTickPrefix), nameof(GameStateManager.OnTick), Priority.Last)
            };
        }

        private static void OnTickPrefix(GameStateManager __instance)
            => MessengerManager.OnGameStateTick(__instance);
    }
}
