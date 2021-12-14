using System;
using System.Linq;
using Diplomacy.DiplomaticAction;
using Diplomacy.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.EncyclopediaItems;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using Widgets;

namespace Diplomacy.ViewModel
{
    internal class DiplomaticBarterFactionVM : SelectorItemVM
    {
        private MBBindingList<DiplomaticBartersFactionRelationshipVM> _allies;
        private MBBindingList<DiplomaticBartersFactionRelationshipVM> _naPacts;
        private MBBindingList<DiplomaticBartersFactionRelationshipVM> _wars;
        public IFaction Faction { get; }
        public ImageIdentifierVM FactionVisual { get; }

        [DataSourceProperty] public string Name { get; }
        [DataSourceProperty] public string FactionLeaderName { get; }
        [DataSourceProperty] public int TotalStrength { get; }
        [DataSourceProperty] public int Relation { get; }
        [DataSourceProperty] public int Fiefs { get; }
        [DataSourceProperty] public string RelationState { get; }
        [DataSourceProperty] public MBBindingList<EncyclopediaTraitItemVM> LeaderTraits { get; }

        [DataSourceProperty]
        public MBBindingList<DiplomaticBartersFactionRelationshipVM> Wars
        {
            get => _wars;
            set
            {
                _wars = value;
                OnPropertyChanged(nameof(Wars));
            }
        }

        [DataSourceProperty]
        public MBBindingList<DiplomaticBartersFactionRelationshipVM> Allies
        {
            get => _allies;
            set
            {
                _allies = value;
                OnPropertyChanged(nameof(Allies));
            }
        }

        [DataSourceProperty]
        public MBBindingList<DiplomaticBartersFactionRelationshipVM> NAPacts
        {
            get => _naPacts;
            set
            {
                _naPacts = value;
                OnPropertyChanged(nameof(NAPacts));
            }
        }

        public DiplomaticBarterFactionVM(IFaction faction) : base(faction.Name)
        {
            Faction = faction;
            Name = faction.Name.ToString();
            FactionLeaderName = faction.Leader.ToString();
            FactionVisual = new ImageIdentifierVM(BannerCode.CreateFrom(faction.Banner), true);
            TotalStrength = Convert.ToInt32(Math.Round(faction.TotalStrength));
            Relation = Convert.ToInt32(faction.Leader.GetRelationWithPlayer());
            RelationState = Relation switch
            {
                >= 50 => Enum.GetName(typeof(ReputationColorWidget.RelationState), ReputationColorWidget.RelationState.VeryHigh)!,
                >= 10 => Enum.GetName(typeof(ReputationColorWidget.RelationState), ReputationColorWidget.RelationState.High)!,
                <= -50 => Enum.GetName(typeof(ReputationColorWidget.RelationState), ReputationColorWidget.RelationState.VeryLow)!,
                <= -10 => Enum.GetName(typeof(ReputationColorWidget.RelationState), ReputationColorWidget.RelationState.Low)!,
                _ => Enum.GetName(typeof(ReputationColorWidget.RelationState), ReputationColorWidget.RelationState.Default)!
            };
            Fiefs = faction.Fiefs.Count;

            Wars = new MBBindingList<DiplomaticBartersFactionRelationshipVM>();
            Allies = new MBBindingList<DiplomaticBartersFactionRelationshipVM>();
            NAPacts = new MBBindingList<DiplomaticBartersFactionRelationshipVM>();


            LeaderTraits = new MBBindingList<EncyclopediaTraitItemVM>();
            foreach (TraitObject traitObject in CampaignUIHelper.GetHeroTraits())
                if (Faction.Leader.GetTraitLevel(traitObject) != 0)
                    LeaderTraits.Add(new EncyclopediaTraitItemVM(traitObject, Faction.Leader));

            Refresh();
        }

        public void Refresh()
        {
            Wars.Clear();
            Allies.Clear();
            NAPacts.Clear();

            foreach (var kingdom in FactionManager.GetEnemyFactions(Faction).OfType<Kingdom>())
                Wars.Add(new DiplomaticBartersFactionRelationshipVM(kingdom));

            foreach (var stanceLink in Faction.Stances.Where(x => x.IsAllied).Where(x => x.Faction1 is Kingdom && x.Faction2 is Kingdom))
            {
                Kingdom kingdom = ((stanceLink.Faction1 == Faction ? stanceLink.Faction2 : stanceLink.Faction1) as Kingdom)!;
                Allies.Add(new DiplomaticBartersFactionRelationshipVM(kingdom));
            }

            foreach (var kingdom in Kingdom.All.Where(x =>
                !x.IsRebelKingdom() && x != Faction && DiplomaticAgreementManager.HasNonAggressionPact((Faction as Kingdom)!, x, out _)))
                NAPacts.Add(new DiplomaticBartersFactionRelationshipVM(kingdom));
        }
    }
}