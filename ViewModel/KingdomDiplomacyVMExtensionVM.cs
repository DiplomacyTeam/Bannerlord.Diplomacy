using DiplomacyFixes.Alliance;
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
		private string _numOfPlayerAlliancesText;

		public KingdomDiplomacyVMExtensionVM(Action<KingdomDecision> forceDecision) : base(forceDecision) {
			this._playerKingdom = (Hero.MainHero.MapFaction as Kingdom);
			this._onSelectionMethodInfo = typeof(KingdomDiplomacyVM).GetMethod("OnDiplomacyItemSelection", BindingFlags.Instance | BindingFlags.NonPublic);
			this.PlayerAlliancesText = new TextObject("Alliances").ToString();
			Events.AllianceFormed.AddNonSerializedListener(this, (x) => RefreshValues());
			CampaignEvents.WarDeclared.AddNonSerializedListener(this, (x,y) => RefreshValues());
			CampaignEvents.MakePeace.AddNonSerializedListener(this, (x,y) => RefreshValues());
			this.RefreshAlliances();
		}

		public override void RefreshValues()
		{
			base.RefreshValues();
			RefreshAlliances();
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
