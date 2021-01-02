using Diplomacy.DiplomaticAction.Alliance;
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
    class KingdomDiplomacyVMExtensionVM : KingdomDiplomacyVM, ICloseableVM
    {
        private MBBindingList<KingdomTruceItemVM> _playerAlliances;
        private Kingdom _playerKingdom;
        private MethodInfo _onSelectionMethodInfo;
        private MethodInfo _executeActionMethodInfo;
        private string _numOfPlayerAlliancesText;
        private bool _showStats;
        private bool _showOverview;

        public KingdomDiplomacyVMExtensionVM(Action<KingdomDecision> forceDecision) : base(forceDecision)
        {
            _playerKingdom = (Hero.MainHero.MapFaction as Kingdom);
            _onSelectionMethodInfo = typeof(KingdomDiplomacyVM).GetMethod("OnDiplomacyItemSelection", BindingFlags.Instance | BindingFlags.NonPublic);
            _executeActionMethodInfo = typeof(KingdomDiplomacyVM).GetMethod("ExecuteAction", BindingFlags.Instance | BindingFlags.NonPublic);
            PlayerAlliancesText = new TextObject("{=zpNalMeA}Alliances").ToString();
            StatsText = new TextObject("{=1occw3EF}Stats").ToString();
            OverviewText = new TextObject("{=OvbY5qxL}Overview").ToString();
            DiplomacyText = new TextObject("{=Q2vXbwvC}Diplomacy").ToString();

            Events.AllianceFormed.AddNonSerializedListener(this, (x) => RefreshValues());
            Events.AllianceBroken.AddNonSerializedListener(this, (x) => RefreshValues());
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, (x, y) =>
            {
                if (Hero.MainHero.MapFaction is Kingdom)
                {
                    RefreshValues();
                }
            });
            CampaignEvents.MakePeace.AddNonSerializedListener(this, (x, y) => RefreshValues());
            RefreshAlliances();
            ExecuteShowOverview();
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

        [DataSourceProperty]
        public bool ShowOverview
        {
            get
            {
                return _showOverview;
            }
            set
            {
                if (value != _showOverview)
                {
                    _showOverview = value;
                    base.OnPropertyChanged("ShowOverview");
                }
            }
        }

        [DataSourceProperty]
        public bool ShowStats
        {
            get
            {
                return _showStats;
            }
            set
            {
                if (value != _showStats)
                {
                    _showStats = value;
                    base.OnPropertyChanged("ShowStats");
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
            BreakAllianceAction.Apply(item.Faction1 as Kingdom, item.Faction2 as Kingdom);
            RefreshDiplomacyList();
            RefreshAlliances();
        }

        private void OnDiplomacyItemSelection(KingdomDiplomacyItemVM item)
        {
            _onSelectionMethodInfo.Invoke(this as KingdomDiplomacyVM, new object[] { item });
        }

        private void ExecuteAction()
        {
            _executeActionMethodInfo.Invoke(this as KingdomDiplomacyVM, new object[] { });
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
            get
            {
                return _numOfPlayerAlliancesText;
            }
            set
            {
                if (value != _numOfPlayerAlliancesText)
                {
                    _numOfPlayerAlliancesText = value;
                    base.OnPropertyChanged("NumOfPlayerAlliancesText");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<KingdomTruceItemVM> PlayerAlliances
        {
            get
            {
                return _playerAlliances;
            }
            set
            {
                if (value != _playerAlliances)
                {
                    _playerAlliances = value;
                    base.OnPropertyChanged("PlayerAlliances");
                }
            }
        }


    }
}
