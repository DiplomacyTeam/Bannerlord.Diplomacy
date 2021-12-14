using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.EncyclopediaItems;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace Diplomacy.ViewModel
{
    internal sealed class DiplomaticBartersFactionRelationshipVM : EncyclopediaFactionVM
    {
        [DataSourceProperty] public HintViewModel Hint { get; set; }

        public DiplomaticBartersFactionRelationshipVM(IFaction faction) : base(faction)
        {
            Hint = new HintViewModel(faction.Name);
        }
    }
}