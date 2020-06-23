using DiplomacyFixes.DiplomaticAction.Alliance;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.ViewModel
{
    class KingdomDiplomacyVMExtensionVM : KingdomDiplomacyVM
    {
		private MBBindingList<KingdomTruceItemVM> _playerAlliances;
		private Kingdom _playerKingdom;
		private MethodInfo _onSelectionMethodInfo;
        private MethodInfo _executeActionMethodInfo;
        private string _numOfPlayerAlliancesText;
        private bool _showStats;
        private bool _showOverview;

        public KingdomDiplomacyVMExtensionVM(Action<KingdomDecision> forceDecision) : base(forceDecision) {
			this._playerKingdom = (Hero.MainHero.MapFaction as Kingdom);
			this._onSelectionMethodInfo = typeof(KingdomDiplomacyVM).GetMethod("OnDiplomacyItemSelection", BindingFlags.Instance | BindingFlags.NonPublic);
            this._executeActionMethodInfo = typeof(KingdomDiplomacyVM).GetMethod("ExecuteAction", BindingFlags.Instance | BindingFlags.NonPublic);
            this.PlayerAlliancesText = new TextObject("Alliances").ToString();
            Events.AllianceFormed.AddNonSerializedListener(this, (x) => RefreshValues());
            Events.AllianceBroken.AddNonSerializedListener(this, (x) => RefreshValues());
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, (x, y) =>
            {
                if (Hero.MainHero.MapFaction is Kingdom)
                {
                    RefreshValues();
                }
            });
            CampaignEvents.MakePeace.AddNonSerializedListener(this, (x,y) => RefreshValues());
			this.RefreshAlliances();
            this.ExecuteShowOverview();
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
            this.ShowOverview = false;
            this.ShowStats = true;
        }

        private void ExecuteShowOverview()
        {
            this.ShowOverview = true;
            this.ShowStats = false;
        }

        [DataSourceProperty]
        public bool ShowOverview
        {
            get
            {
                return this._showOverview;
            }
            set
            {
                if (value != this._showOverview)
                {
                    this._showOverview = value;
                    base.OnPropertyChanged("ShowOverview");
                }
            }
        }

        [DataSourceProperty]
        public bool ShowStats
        {
            get
            {
                return this._showStats;
            }
            set
            {
                if (value != this._showStats)
                {
                    this._showStats = value;
                    base.OnPropertyChanged("ShowStats");
                }
            }
        }

		private void RefreshAlliances()
		{
			if (this.PlayerAlliances == null)
			{
				this.PlayerAlliances = new MBBindingList<KingdomTruceItemVM>();
			}

			this.PlayerAlliances.Clear();

			foreach (Kingdom kingdom in Kingdom.All)
			{
				if (kingdom != this._playerKingdom && !kingdom.IsEliminated && (FactionManager.IsAlliedWithFaction(kingdom, this._playerKingdom)))
				{
					this.PlayerAlliances.Add(new KingdomAllianceItemVM(this._playerKingdom, kingdom, this.OnDiplomacyItemSelection, this.BreakAlliance));
                }
            }


            GameTexts.SetVariable("STR", this.PlayerAlliances.Count);
            this.NumOfPlayerAlliancesText = GameTexts.FindText("str_STR_in_parentheses", null).ToString();
        }

        private void BreakAlliance(KingdomDiplomacyItemVM item)
        {
            BreakAllianceAction.Apply(item.Faction1 as Kingdom, item.Faction2 as Kingdom);
            RefreshDiplomacyList();
            RefreshAlliances();
        }

        private void OnDiplomacyItemSelection(KingdomDiplomacyItemVM item)
        {
            this._onSelectionMethodInfo.Invoke(this as KingdomDiplomacyVM, new object[] { item });
        }

        private void ExecuteAction()
        {
            this._executeActionMethodInfo.Invoke(this as KingdomDiplomacyVM, new object[] { });
        }

        [DataSourceProperty]
        public string PlayerAlliancesText { get; }

        [DataSourceProperty]
        public string NumOfPlayerAlliancesText
        {
            get
            {
                return this._numOfPlayerAlliancesText;
            }
            set
            {
                if (value != this._numOfPlayerAlliancesText)
                {
                    this._numOfPlayerAlliancesText = value;
                    base.OnPropertyChanged("NumOfPlayerAlliancesText");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<KingdomTruceItemVM> PlayerAlliances
        {
            get
            {
                return this._playerAlliances;
            }
            set
            {
                if (value != this._playerAlliances)
                {
                    this._playerAlliances = value;
                    base.OnPropertyChanged("PlayerAlliances");
                }
            }
        }


    }
}
