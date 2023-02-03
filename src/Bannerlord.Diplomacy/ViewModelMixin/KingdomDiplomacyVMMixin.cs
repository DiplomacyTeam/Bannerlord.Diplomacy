using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;

using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.Events;
using Diplomacy.Extensions;

using JetBrains.Annotations;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModelMixin
{
    [ViewModelMixin(nameof(KingdomDiplomacyVM.RefreshValues))]
    [UsedImplicitly]
    internal sealed class KingdomDiplomacyVMMixin : BaseViewModelMixin<KingdomDiplomacyVM>
    {
        private static readonly TextObject _TAlliances = new("{=zpNalMeA}Alliances");
        private static readonly TextObject _TStats = new("{=1occw3EF}Stats");
        private static readonly TextObject _TOverview = new("{=OvbY5qxL}Overview");
        private static readonly TextObject _TDiplomacy = new("{=Q2vXbwvC}Diplomacy");


        private string _numOfPlayerAlliancesText = null!;


        private MBBindingList<KingdomTruceItemVM> _playerAlliances;
        private bool _showOverview;
        private bool _showStats;

        [DataSourceProperty]
        public bool ShowOverview { get => _showOverview; set => SetField(ref _showOverview, value, nameof(ShowOverview)); }

        [DataSourceProperty]
        public bool ShowStats { get => _showStats; set => SetField(ref _showStats, value, nameof(ShowStats)); }

        [DataSourceProperty]
        public string PlayerAlliancesText { get; }

        [DataSourceProperty]
        public string StatsText { get; }

        [DataSourceProperty]
        public string OverviewText { get; }

        [DataSourceProperty]
        public string DiplomacyText { get; }

        [DataSourceProperty]
        public string NumOfPlayerAlliancesText { get => _numOfPlayerAlliancesText; set => SetField(ref _numOfPlayerAlliancesText, value, nameof(NumOfPlayerAlliancesText)); }

        [DataSourceProperty]
        public MBBindingList<KingdomTruceItemVM> PlayerAlliances { get => _playerAlliances; set => SetField(ref _playerAlliances, value, nameof(PlayerAlliances)); }

        public KingdomDiplomacyVMMixin(KingdomDiplomacyVM vm) : base(vm)
        {
            _playerAlliances = new MBBindingList<KingdomTruceItemVM>();
            PlayerAlliancesText = _TAlliances.ToString();
            StatsText = _TStats.ToString();
            OverviewText = _TOverview.ToString();
            DiplomacyText = _TDiplomacy.ToString();

            // No refresh needed on NAP because it doesn't move the item from one diplomacy group (At War / Alliances / At Peace) to another
            DiplomacyEvents.AllianceFormed.AddNonSerializedListener(this, _ => ViewModel!.RefreshValues());
            DiplomacyEvents.AllianceBroken.AddNonSerializedListener(this, _ => ViewModel!.RefreshValues());
#if v100 || v101 || v102 || v103
            CampaignEvents.MakePeace.AddNonSerializedListener(this, (_, _) => ViewModel!.RefreshValues());
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, (_, _) =>
#else
            CampaignEvents.MakePeace.AddNonSerializedListener(this, (_, _, _) => ViewModel!.RefreshValues());
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, (_, _, _) =>
#endif
            {
                if (Hero.MainHero.MapFaction is Kingdom)
                    ViewModel!.RefreshValues();
            });

            OnRefresh();
        }

        public override void OnFinalize()
        {
            DiplomacyEvents.RemoveListeners(this);
            CampaignEventDispatcher.Instance.RemoveListeners(this);
        }

        [DataSourceMethod]
        [UsedImplicitly]
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

        public override void OnRefresh()
        {
            ExecuteShowOverview();
            RemoveRebelKingdoms(ViewModel!.PlayerTruces);
            RemoveRebelKingdoms(ViewModel!.PlayerWars);

            var alliances = ViewModel!.PlayerTruces.Where(item => item.Faction1.IsAlliedWith(item.Faction2)).ToList();
            foreach (var alliance in alliances) ViewModel!.PlayerTruces.Remove(alliance);

            foreach (var truce in ViewModel!.PlayerTruces.ToList())
            {
                var otherKingdom = truce.Faction2 as Kingdom;
                if (otherKingdom!.IsRebelKingdom()) ViewModel!.PlayerTruces.Remove(truce);
            }

            RefreshAlliances(alliances);

            GameTexts.SetVariable("STR", ViewModel!.PlayerTruces.Count);
            ViewModel!.NumOfPlayerTrucesText = GameTexts.FindText("str_STR_in_parentheses").ToString();
            GameTexts.SetVariable("STR", ViewModel!.PlayerWars.Count);
            ViewModel!.NumOfPlayerWarsText = GameTexts.FindText("str_STR_in_parentheses").ToString();
        }

        private void RemoveRebelKingdoms<T>(MBBindingList<T> items) where T : KingdomDiplomacyItemVM
        {
            foreach (var item in items.ToList())
            {
                var otherKingdom = item.Faction2 as Kingdom;
                if (otherKingdom!.IsRebelKingdom()) items.Remove(item);
            }
        }

        private void RefreshAlliances(List<KingdomTruceItemVM> alliances)
        {
            PlayerAlliances.Clear();

            foreach (var alliance in alliances) PlayerAlliances.Add(alliance);

            GameTexts.SetVariable("STR", PlayerAlliances.Count);
            NumOfPlayerAlliancesText = GameTexts.FindText("str_STR_in_parentheses").ToString();
        }

        [DataSourceMethod]
        [UsedImplicitly]
        public void BreakAlliance(KingdomDiplomacyItemVM item)
        {
            BreakAllianceAction.Apply((Kingdom) item.Faction1, (Kingdom) item.Faction2);
            ViewModel!.RefreshDiplomacyList();
            OnRefresh();
        }
    }
}