using System;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    class DeclareWarCooldown : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.MakePeace.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.RegisterDeclareWarCooldown));
        }

        private void RegisterDeclareWarCooldown(IFaction faction1, IFaction faction2)
        {
            if (Hero.MainHero.MapFaction.Equals(faction1) || Hero.MainHero.MapFaction.Equals(faction2))
            {
                IFaction factionToUpdate = Hero.MainHero.MapFaction.Equals(faction1) ? faction2 : faction1;
                CooldownManager.UpdateLastWarTime(factionToUpdate, CampaignTime.Now);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
