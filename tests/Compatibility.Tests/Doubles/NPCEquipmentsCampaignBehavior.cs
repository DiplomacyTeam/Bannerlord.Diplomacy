using Helpers;

using System.Runtime.CompilerServices;

using TaleWorlds.Core;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors
{
    public sealed class NPCEquipmentsCampaignBehavior
    {
        public Equipment? NewBattle { get; set; }
        public Equipment? NewCivilian { get; set; }
        public Equipment? OldBattle { get; set; }
        public Equipment? OldCivilian { get; set; }
        public bool Completed { get; private set; }

        public void RaiseRulingClanChanged(Kingdom kingdom, Clan? oldRulingClan)
            => OnRulingClanChanged(kingdom, oldRulingClan);

        // Exercise the real Harmony transpiler against the four assignment call sites and
        // player/inactive-ruler branches used by Bannerlord, without needing a running campaign.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void OnRulingClanChanged(Kingdom kingdom, Clan? oldRulingClan)
        {
            if (kingdom.Leader != Hero.MainHero)
            {
                EquipmentHelper.AssignHeroEquipmentFromEquipment(kingdom.Leader, NewBattle!);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(kingdom.Leader, NewCivilian!);
            }

            var oldRuler = oldRulingClan?.Leader;
            if (oldRuler is not null && oldRuler.IsActive && oldRuler != Hero.MainHero)
            {
                EquipmentHelper.AssignHeroEquipmentFromEquipment(oldRuler, OldBattle!);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(oldRuler, OldCivilian!);
            }

            Completed = true;
        }
    }
}
