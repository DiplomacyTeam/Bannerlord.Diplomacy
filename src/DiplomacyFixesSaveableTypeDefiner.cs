using DiplomacyFixes.DiplomaticAction;
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
            base.AddClassDefinition(typeof(DiplomaticAgreement), 5);
            base.AddClassDefinition(typeof(NonAggressionPactAgreement), 6);
            base.AddClassDefinition(typeof(DiplomaticAgreementManager), 7);
            base.AddClassDefinition(typeof(ExpansionismManager), 8);
        }

        protected override void DefineStructTypes()
        {
            base.DefineStructTypes();
            base.AddStructDefinition(typeof(FactionMapping), 1000);
        }

        protected override void DefineContainerDefinitions()
        {
            base.ConstructContainerDefinition(typeof(List<Messenger>));
            base.ConstructContainerDefinition(typeof(Dictionary<IFaction, CampaignTime>));
            base.ConstructContainerDefinition(typeof(Dictionary<Kingdom, CampaignTime>));
            base.ConstructContainerDefinition(typeof(List<DiplomaticAgreement>));
            base.ConstructContainerDefinition(typeof(Dictionary<FactionMapping, List<DiplomaticAgreement>>));
        }
    }
}
