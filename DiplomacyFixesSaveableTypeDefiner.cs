using DiplomacyFixes.Messengers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
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
        }

        protected override void DefineContainerDefinitions()
        {
            base.ConstructContainerDefinition(typeof(List<Messenger>));
            base.ConstructContainerDefinition(typeof(Dictionary<IFaction, CampaignTime>));
            base.ConstructContainerDefinition(typeof(Dictionary<Kingdom, CampaignTime>));
        }
    }
}
