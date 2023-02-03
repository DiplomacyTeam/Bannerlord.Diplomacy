#if v100 || v101 || v102 || v103
using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.Events;
using Diplomacy.PatchTools;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace Diplomacy.Patches
{
    /// <summary>
    /// Fires the WarDeclaredEvent when a war is declared.
    /// </summary>
    internal sealed class DeclareWarActionPatch : PatchClass<DeclareWarActionPatch>
    {
        private static readonly Type TargetType = typeof(DeclareWarAction);

        protected override IEnumerable<Patch> Prepare() => new Patch[]
        {
            new Postfix(nameof(ApplyPostfix), TargetType, nameof(DeclareWarAction.Apply)),
            new Postfix(nameof(ApplyDeclareWarOverProvocationPostfix), TargetType, nameof(DeclareWarAction.ApplyDeclareWarOverProvocation)),
        };

        private static void ApplyPostfix(IFaction faction1, IFaction faction2)
            => DiplomacyEvents.Instance.OnWarDeclared(new WarDeclaredEvent(faction1, faction2, false));

        private static void ApplyDeclareWarOverProvocationPostfix(IFaction faction, IFaction provocatorFaction)
            => DiplomacyEvents.Instance.OnWarDeclared(new WarDeclaredEvent(faction, provocatorFaction, true));
    }
}
#endif