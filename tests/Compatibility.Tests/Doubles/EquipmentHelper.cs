using System.Runtime.CompilerServices;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Helpers
{
    public static class EquipmentHelper
    {
        // Reproduces the game's unchecked dereference and observable equipment update.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AssignHeroEquipmentFromEquipment(Hero hero, Equipment equipment)
        {
            var current = equipment.IsCivilian ? hero.CivilianEquipment : hero.BattleEquipment;
            current.Outfit = equipment.Outfit;
        }
    }
}
