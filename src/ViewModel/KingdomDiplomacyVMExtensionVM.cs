using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.Event;
using HarmonyLib;
using System;
using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    internal sealed class KingdomDiplomacyVMExtensionVM : KingdomDiplomacyVM, ICloseableVM
    {
        private static readonly TextObject _TAlliances = new("{=zpNalMeA}Alliances");
        private static readonly TextObject _TStats = new("{=1occw3EF}Stats");
        private static readonly TextObject _TOverview = new("{=OvbY5qxL}Overview");
        private static readonly TextObject _TDiplomacy = new("{=Q2vXbwvC}Diplomacy");


        private delegate void OnDiplomacyItemSelectionDel(KingdomDiplomacyVM diplomacyVM, KingdomDiplomacyItemVM item);
        private static readonly OnDiplomacyItemSelectionDel OnDiplomacyItemSelectionParent = AccessTools.MethodDelegate<OnDiplomacyItemSelectionDel>(AccessTools.Method(typeof(KingdomDiplomacyVM), "OnDiplomacyItemSelection"));
        private static readonly Action<KingdomDiplomacyVM> ExecuteActionParent = AccessTools.MethodDelegate<Action<KingdomDiplomacyVM>>(AccessTools.Method(typeof(KingdomDiplomacyVM), "ExecuteAction"));

        private MBBindingList<KingdomTruceItemVM> _playerAlliances;
        private readonly Kingdom _playerKingdom;
        private string _numOfPlayerAlliancesText;
        private bool _showStats;
        private bool _showOverview;

        public KingdomDiplomacyVMExtensionVM(Action<KingdomDecision> forceDecision) : base(forceDecision)
        {
            _playerKingdom = Hero.MainHero.Clan.Kingdom;

            PlayerAlliancesText = _TAlliances.ToString();
            StatsText = _TStats.ToString();
            OverviewText = _TOverview.ToString();
            DiplomacyText = _TDiplomacy.ToString();

            // No refresh needed on NAP because it doesn't move the item from one diplomacy group (At War / Alliances / At Peace) to another
            Events.AllianceFormed.AddNonSerializedListener(this, (e) => RefreshValues());
            Events.AllianceBroken.AddNonSerializedListener(this, (e) => RefreshValues());
            CampaignEvents.MakePeace.AddNonSerializedListener(this, (f1, f2) => RefreshValues());
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, (f1, f2) =>
            {
                if (Hero.MainHero.MapFaction is Kingdom)
                    RefreshValues();
            });

            RefreshAlliances();
            ExecuteShowOverview();
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

        private void ExecuteAction()
        {
            ExecuteActionParent(this);
        }

        public void OnClose()
        {
            Events.RemoveListeners(this);
            CampaignEvents.RemoveListeners(this);
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
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
                    OnPropertyChanged(nameof(ShowOverview));
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
                    OnPropertyChanged(nameof(ShowStats));
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
            RefreshDiplomacyList();
            RefreshAlliances();
        }

        private void OnDiplomacyItemSelection(KingdomDiplomacyItemVM item)
        {
            OnDiplomacyItemSelectionParent(this, item);
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
                    OnPropertyChanged(nameof(NumOfPlayerAlliancesText));
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
                    OnPropertyChanged(nameof(PlayerAlliances));
                }
            }
        }
    }
}
