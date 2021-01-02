using Diplomacy.DiplomaticAction;
using Diplomacy.Messengers;

using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace Diplomacy
{
    internal class CustomSavedTypeDefiner : SaveableTypeDefiner
    {
        private const int SaveBaseId = 1_984_110_150;

        public CustomSavedTypeDefiner() : base(SaveBaseId) { }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(Messenger), 1);
            AddClassDefinition(typeof(WarExhaustionManager), 2);
            AddClassDefinition(typeof(CooldownManager), 3);
            AddClassDefinition(typeof(MessengerManager), 4);
            AddClassDefinition(typeof(DiplomaticAgreement), 5);
            AddClassDefinition(typeof(NonAggressionPactAgreement), 6);
            AddClassDefinition(typeof(DiplomaticAgreementManager), 7);
            AddClassDefinition(typeof(ExpansionismManager), 8);
        }

        protected override void DefineStructTypes() => AddStructDefinition(typeof(FactionPair), 9);

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<Messenger>));
            ConstructContainerDefinition(typeof(Dictionary<IFaction, CampaignTime>));
            ConstructContainerDefinition(typeof(Dictionary<Kingdom, CampaignTime>));
            ConstructContainerDefinition(typeof(List<DiplomaticAgreement>));
            ConstructContainerDefinition(typeof(Dictionary<FactionPair, List<DiplomaticAgreement>>));
        }
    }
}
