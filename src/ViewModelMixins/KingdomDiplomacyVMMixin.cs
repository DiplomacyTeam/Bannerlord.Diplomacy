using System.Collections.Generic;
using System.Linq;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.Event;
using HarmonyLib;
using System.Reflection;
using Diplomacy.Extensions;
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

        private static readonly MethodInfo _OnDiplomacyItemSelection =
            AccessTools.DeclaredMethod(typeof(KingdomDiplomacyVM), "OnDiplomacyItemSelection");
        private static readonly MethodInfo _SetDefaultSelectedItem =
            AccessTools.DeclaredMethod(typeof(KingdomDiplomacyVM), "SetDefaultSelectedItem");


        private MBBindingList<KingdomTruceItemVM> _playerAlliances;
        private readonly Kingdom _playerKingdom;
        private string _numOfPlayerAlliancesText;
        private bool _showStats;
        private bool _showOverview;
        private readonly PropertyChangedWithValueEventHandler _eventHandler;

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

            OnRefresh();
            _eventHandler = new PropertyChangedWithValueEventHandler(OnPropertyChangedWithValue);

            ViewModel!.PropertyChangedWithValue += _eventHandler;
        }

        private void OnPropertyChangedWithValue(object sender, PropertyChangedWithValueEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.CurrentSelectedDiplomacyItem))
            {
#if !(e159 || e1510)
                OnDiplomacyItemSelection(ViewModel!.CurrentSelectedDiplomacyItem);
#endif
            }
        }

        public override void OnFinalize()
        {
            Events.RemoveListeners(this);
            ViewModel!.PropertyChangedWithValue -= _eventHandler;
#if e159
            CampaignEvents.RemoveListeners(this);
#else
            CampaignEventDispatcher.Instance.RemoveListeners(this);
#endif
        }
        [DataSourceMethod]
        public void ExecuteShowStats()
        {
            ShowOverview = false;
            ShowStats = true;
        }
        [DataSourceMethod]
        public void ExecuteShowOverview()
        {
            ShowOverview = true;
            ShowStats = false;
        }

#if !(e159 || e1510)
        public void ExecuteShowStatComparison()
        {
            ViewModel!.IsDisplayingStatComparisons = true;
            ViewModel!.IsDisplayingWarLogs = false;
        }
#endif

        private void OnDiplomacyItemSelection(KingdomDiplomacyItemVM item)
        {
            _OnDiplomacyItemSelection.Invoke(ViewModel, new object[] { item });
#if !(e159 || e1510)
            ExecuteShowStatComparison();
#endif
        }

        public override void OnRefresh()
        {
            ExecuteShowOverview();
            RemoveRebelKingdoms(ViewModel!.PlayerTruces);
            RemoveRebelKingdoms(ViewModel!.PlayerWars);

            var alliances = ViewModel!.PlayerTruces.Where(item => item.Faction1.IsAlliedWith(item.Faction2)).ToList();
            foreach (KingdomTruceItemVM alliance in alliances)
            {
                ViewModel!.PlayerTruces.Remove(alliance);
            }

            foreach (var truce in ViewModel!.PlayerTruces.ToList())
            {
                var otherKingdom = truce.Faction2 as Kingdom;
                if (otherKingdom!.IsRebelKingdom())
                {
                    ViewModel!.PlayerTruces.Remove(truce);
                }
            }

            RefreshAlliances(alliances);

            GameTexts.SetVariable("STR", ViewModel!.PlayerTruces.Count);
            ViewModel!.NumOfPlayerTrucesText = GameTexts.FindText("str_STR_in_parentheses", null).ToString();
            GameTexts.SetVariable("STR", ViewModel!.PlayerWars.Count);
            ViewModel!.NumOfPlayerWarsText = GameTexts.FindText("str_STR_in_parentheses", null).ToString();

            _SetDefaultSelectedItem.Invoke(ViewModel!, new object[] { });
        }

        private void RemoveRebelKingdoms<T>(MBBindingList<T> items) where T : KingdomDiplomacyItemVM
        {
            foreach (var item in items.ToList())
            {
                var otherKingdom = item.Faction2 as Kingdom;
                if (otherKingdom!.IsRebelKingdom())
                {
                    items.Remove(item);
                }
            }
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

        private void RefreshAlliances(List<KingdomTruceItemVM> alliances)
        {
            PlayerAlliances ??= new MBBindingList<KingdomTruceItemVM>();

            PlayerAlliances.Clear();

            foreach (var alliance in alliances)
            {
                PlayerAlliances.Add(alliance);
            }

            GameTexts.SetVariable("STR", PlayerAlliances.Count);
            NumOfPlayerAlliancesText = GameTexts.FindText("str_STR_in_parentheses", null).ToString();
        }

        private void BreakAlliance(KingdomDiplomacyItemVM item)
        {
            BreakAllianceAction.Apply((Kingdom)item.Faction1, (Kingdom)item.Faction2);
            ViewModel!.RefreshDiplomacyList();
            OnRefresh();
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
