using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.Event;
using Diplomacy.ViewModel;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModelMixins
{
    [ViewModelMixin(nameof(KingdomDiplomacyVM.RefreshValues))]
    internal sealed class KingdomDiplomacyVMMixin : BaseViewModelMixin<KingdomDiplomacyVM>
    {

        private static readonly TextObject _TAlliances = new("{=zpNalMeA}Alliances");
        private static readonly TextObject _TStats = new("{=1occw3EF}Stats");
        private static readonly TextObject _TOverview = new("{=OvbY5qxL}Overview");
        private static readonly TextObject _TDiplomacy = new("{=Q2vXbwvC}Diplomacy");


        private MBBindingList<KingdomTruceItemVM> _playerAlliances;
        private readonly Kingdom _playerKingdom;
        private string _numOfPlayerAlliancesText;
        private bool _showStats;
        private bool _showOverview;

        public KingdomDiplomacyVMMixin(KingdomDiplomacyVM vm) : base(vm)
        {
            _playerKingdom = Hero.MainHero.Clan.Kingdom;

            PlayerAlliancesText = _TAlliances.ToString();
            StatsText = _TStats.ToString();
            OverviewText = _TOverview.ToString();
            DiplomacyText = _TDiplomacy.ToString();

            // No refresh needed on NAP because it doesn't move the item from one diplomacy group (At War / Alliances / At Peace) to another
            Events.AllianceFormed.AddNonSerializedListener(this, (e) => ViewModel!.RefreshValues());
            Events.AllianceBroken.AddNonSerializedListener(this, (e) => ViewModel!.RefreshValues());
            CampaignEvents.MakePeace.AddNonSerializedListener(this, (f1, f2) => ViewModel!.RefreshValues());
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, (f1, f2) =>
            {
                if (Hero.MainHero.MapFaction is Kingdom)
                    ViewModel!.RefreshValues();
            });

            RefreshAlliances();

            ViewModel!.PropertyChangedWithValue += new PropertyChangedWithValueEventHandler(OnPropertyChangedWithValue);
        }

        private void OnPropertyChangedWithValue(object sender, PropertyChangedWithValueEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.CurrentSelectedDiplomacyItem))
            {
                OnDiplomacyItemSelection(ViewModel!.CurrentSelectedDiplomacyItem);
            }
        }

        public override void OnFinalize()
        {
            Events.RemoveListeners(this);
#if e159
            CampaignEvents.RemoveListeners(this);
#else
            CampaignEventDispatcher.Instance.RemoveListeners(this);
#endif
        }

        private void ExecuteShowStats()
        {
            ShowOverview = false;
            ShowStats = true;
        }

        private void ExecuteShowOverview()
        {
            ShowOverview = true;
            ShowStats = false;
        }

#if !(e159 || e1510)
        private void ExecuteShowStatComparison()
        {
            ViewModel!.IsDisplayingStatComparisons = true;
            ViewModel!.IsDisplayingWarLogs = false;
        }

        private void OnDiplomacyItemSelection(KingdomDiplomacyItemVM item)
        {
            ExecuteShowStatComparison();
        }
#endif

        public override void OnRefresh()
        {
            RefreshAlliances();
        }

        [DataSourceProperty]
        public bool ShowOverview
        {
            get => _showOverview;
            set
            {
                if (value != _showOverview)
                {
                    _showOverview = value;
                    ViewModel!.OnPropertyChanged(nameof(ShowOverview));
                }
            }
        }

        [DataSourceProperty]
        public bool ShowStats
        {
            get => _showStats;
            set
            {
                if (value != _showStats)
                {
                    _showStats = value;
                    ViewModel!.OnPropertyChanged(nameof(ShowStats));
                }
            }
        }

        private void RefreshAlliances()
        {
            if (PlayerAlliances is null)
            {
                PlayerAlliances = new MBBindingList<KingdomTruceItemVM>();
            }

            PlayerAlliances.Clear();

            foreach (var kingdom in Kingdom.All)
            {
                if (kingdom != _playerKingdom && !kingdom.IsEliminated && (FactionManager.IsAlliedWithFaction(kingdom, _playerKingdom)))
                {
                    PlayerAlliances.Add(new KingdomAllianceItemVM(_playerKingdom, kingdom, OnDiplomacyItemSelection, BreakAlliance));
                }
            }


            GameTexts.SetVariable("STR", PlayerAlliances.Count);
            NumOfPlayerAlliancesText = GameTexts.FindText("str_STR_in_parentheses", null).ToString();
        }

        private void BreakAlliance(KingdomDiplomacyItemVM item)
        {
            BreakAllianceAction.Apply((Kingdom)item.Faction1, (Kingdom)item.Faction2);
            ViewModel!.RefreshDiplomacyList();
            RefreshAlliances();
        }

        [DataSourceProperty]
        public string PlayerAlliancesText { get; }

        [DataSourceProperty]
        public string StatsText { get; }

        [DataSourceProperty]
        public string OverviewText { get; }

        [DataSourceProperty]
        public string DiplomacyText { get; }

        [DataSourceProperty]
        public string NumOfPlayerAlliancesText
        {
            get => _numOfPlayerAlliancesText;
            set
            {
                if (value != _numOfPlayerAlliancesText)
                {
                    _numOfPlayerAlliancesText = value;
                    ViewModel!.OnPropertyChanged(nameof(NumOfPlayerAlliancesText));
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<KingdomTruceItemVM> PlayerAlliances
        {
            get => _playerAlliances;
            set
            {
                if (value != _playerAlliances)
                {
                    _playerAlliances = value;
                    ViewModel!.OnPropertyChanged(nameof(PlayerAlliances));
                }
            }
        }
    }
}
