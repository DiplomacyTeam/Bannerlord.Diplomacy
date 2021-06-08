using Diplomacy.CivilWar;
using Diplomacy.CivilWar.Factions;
using Diplomacy.Costs;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.EncyclopediaItems;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    internal class RebelFactionsVM : TaleWorlds.Library.ViewModel
    {
        private Action _onComplete;
        private MBBindingList<RebelFactionItemVM> _rebelFactionItems;
        private EncyclopediaFactionVM _kingdomDisplay;
        private bool _shouldShowCreateFaction;
        private Kingdom _kingdom;

        private DiplomacyCost _createFactionCost;
        private HintViewModel _createFactionHint;

        [DataSourceProperty]
        public string FactionsLabel { get; set; }

        [DataSourceProperty]
        public string CreateFactionLabel { get; set; }

        [DataSourceProperty]
        public int CreateFactionInfluenceCost { get; set; }

        public RebelFactionsVM(Kingdom kingdom, Action onComplete)
        {
            _onComplete = onComplete;
            RebelFactionItems = new();
            FactionsLabel = new TextObject(StringConstants.Factions).ToString();
            CreateFactionLabel = new TextObject("{=hBSo0Ziq}Create Faction").ToString();
            _kingdom = kingdom;
            _createFactionCost = new InfluenceCost(Clan.PlayerClan, Settings.Instance!.FactionCreationInfluenceCost);
            CreateFactionInfluenceCost = Settings.Instance!.FactionCreationInfluenceCost;
            this.RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            RebelFactionItems.Clear();
            foreach (RebelFaction rebelFaction in RebelFactionManager.GetRebelFaction(_kingdom))
                RebelFactionItems.Add(new RebelFactionItemVM(rebelFaction, _onComplete, this.RefreshValues));
            var mainHeroIsClanSponsor = RebelFactionItems.Where(factionItem => factionItem.RebelFaction.SponsorClan == Clan.PlayerClan).Any();

            var canCreateFaction = CreateFactionAction.CanApply(Clan.PlayerClan, default, out TextObject? reason);

            ShouldShowCreateFaction = Clan.PlayerClan.Kingdom == _kingdom && canCreateFaction;
            CreateFactionHint = GenerateCreateFactionHint(canCreateFaction, reason);
        }

        private HintViewModel GenerateCreateFactionHint(bool canCreate, TextObject? reason)
        {
            if (Clan.PlayerClan.Kingdom == _kingdom && reason != null)
            {
                return new HintViewModel(reason);
            }
            else
            {
                return new HintViewModel();
            }
        }

        public void OnComplete() => _onComplete();

        public void OnCreateFaction()
        {
            List<InquiryElement> inquiryElements = new();
            foreach (int value in Enum.GetValues(typeof(RebelDemandType)))
            {
                var demandType = (RebelDemandType)value;
                inquiryElements.Add(new InquiryElement(demandType, demandType.GetName(), null, CreateFactionAction.CanApply(Clan.PlayerClan, demandType, out _), demandType.GetHint()));
            }

            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                CreateFactionLabel,
                new TextObject("{=2PglxF8k}Choose Faction Demand").ToString(),
                inquiryElements,
                true,
                1,
                GameTexts.FindText("str_ok").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                this.HandleCreateFaction,
                null
                ), true);
        }

        private void HandleCreateFaction(List<InquiryElement> inquiryElements)
        {
            object identifier = inquiryElements.First().Identifier;

            // canceled 
            if (identifier == null)
            {
                return;
            }

            var rebelDemandType = (RebelDemandType)identifier;

            RebelFaction rebelFaction;

            switch (rebelDemandType)
            {
                case RebelDemandType.Secession:
                    rebelFaction = new SecessionFaction(Clan.PlayerClan);
                    break;
                case RebelDemandType.Abdication:
                    rebelFaction = new AbdicationFaction(Clan.PlayerClan);
                    break;
                default:
                    throw new MBException("Should have a type of demand when creating a faction");
            }

            CreateFactionAction.Apply(rebelFaction);
            this.RefreshValues();
        }

        [DataSourceProperty]
        public MBBindingList<RebelFactionItemVM> RebelFactionItems
        {
            get => _rebelFactionItems;
            set
            {
                if (value != _rebelFactionItems)
                {
                    _rebelFactionItems = value;
                    OnPropertyChanged(nameof(RebelFactionItems));
                }
            }
        }

        [DataSourceProperty]
        public EncyclopediaFactionVM KingdomDisplay
        {
            get => _kingdomDisplay;
            set
            {
                if (value != _kingdomDisplay)
                {
                    _kingdomDisplay = value;
                    OnPropertyChanged(nameof(KingdomDisplay));
                }
            }
        }

        [DataSourceProperty]
        public bool ShouldShowCreateFaction
        {
            get => _shouldShowCreateFaction;
            set
            {
                if (value != _shouldShowCreateFaction)
                {
                    _shouldShowCreateFaction = value;
                    OnPropertyChanged(nameof(ShouldShowCreateFaction));
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel CreateFactionHint
        {
            get => _createFactionHint;
            set
            {
                if (value != _createFactionHint)
                {
                    _createFactionHint = value;
                    OnPropertyChanged(nameof(CreateFactionHint));
                }
            }
        }
    }
}