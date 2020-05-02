using DiplomacyFixes.Messengers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes
{
    internal class DiplomacyFixesSaveableTypeDefiner : SaveableTypeDefiner
    {
        public DiplomacyFixesSaveableTypeDefiner() : base(2051383445) { }

        protected override void DefineClassTypes()
        {
            base.AddClassDefinition(typeof(Messenger), 1);
            base.AddClassDefinition(typeof(WarExhaustionManager), 2);
            base.AddClassDefinition(typeof(CooldownManager), 3);
            base.AddClassDefinition(typeof(MessengerManager), 4);
        }

        protected override void DefineContainerDefinitions()
        {
            base.ConstructContainerDefinition(typeof(List<Messenger>));
            base.ConstructContainerDefinition(typeof(Dictionary<IFaction, CampaignTime>));
            base.ConstructContainerDefinition(typeof(Dictionary<Kingdom, CampaignTime>));
        }
    }
}
