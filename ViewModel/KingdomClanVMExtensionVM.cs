using DiplomacyFixes.GauntletInterfaces;
using DiplomacyFixes.GrantFief;
using System;
using System.ComponentModel;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomClan;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.ViewModel
{
    class KingdomClanVMExtensionVM : KingdomClanVM, INotifyPropertyChanged, ICloseableVM
    {
        private bool _canGrantFiefToClan;
        private GrantFiefInterface _grantFiefInterface;
        private HintViewModel _grantFiefHint;

        private Action _executeExpel;
        private Action _executeSupport;
        private bool _canDonateGold;

        public KingdomClanVMExtensionVM(Action<TaleWorlds.CampaignSystem.Election.KingdomDecision> forceDecide) : base(forceDecide)
        {
            this._executeExpel = () => typeof(KingdomClanVM).GetMethod("ExecuteExpelCurrentClan", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { });
            this._executeSupport = () => typeof(KingdomClanVM).GetMethod("ExecuteSupport", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { });

            Events.FiefGranted.AddNonSerializedListener(this, this.RefreshCanGrantFief);
            this._grantFiefInterface = new GrantFiefInterface();
            this.GrantFiefActionName = new TextObject("{=LpoyhORp}Grant Fief").ToString();
            this.GrantFiefExplanationText = new TextObject("{=98hwXUTp}Grant fiefs to clans in your kingdom").ToString();
            this.DonateGoldActionName = new TextObject("Donate Gold").ToString();
            base.PropertyChanged += new PropertyChangedEventHandler(this.OnPropertyChanged);
            RefreshCanGrantFief();
            RefreshCanDonateGold();
        }

        public void OnClose()
        {
            Events.RemoveListeners(this);
        }

        private void ExecuteExpelCurrentClan()
        {
            this._executeExpel();
        }

        private void ExecuteSupport()
        {
            this._executeSupport();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentSelectedClan")
            {
                RefreshCanGrantFief();
            }
        }

        public void GrantFief()
        {
            this._grantFiefInterface.ShowFiefInterface(ScreenManager.TopScreen, this.CurrentSelectedClan.Clan.Leader);
        }

        private void DonateGold()
        {
            GiveGoldToClanAction.ApplyFromHeroToClan(Hero.MainHero, this.CurrentSelectedClan.Clan, DonationAmount);
            RefreshCanDonateGold();
        }


        private void RefreshCanGrantFief(Town town)
        {
            base.RefreshClan();
            RefreshCanGrantFief();
        }

        private void RefreshCanGrantFief()
        {
            this.CanGrantFiefToClan = GrantFiefAction.CanGrantFief(this.CurrentSelectedClan.Clan, out string hint);
            this.GrantFiefHint = this.CanGrantFiefToClan ? new HintViewModel() : new HintViewModel(hint, null);
        }

        private void RefreshCanDonateGold()
        {
            this.CanDonateGold = Hero.MainHero.Gold >= DonationAmount;
        }

        [DataSourceProperty]
        public bool CanDonateGold
        {
            get { return this._canDonateGold; }

            set
            {
                if (value != this._canDonateGold)
                {
                    this._canDonateGold = value;
                    base.OnPropertyChanged("CanDonateGold");
                }
            }
        }

        [DataSourceProperty]
        public bool CanGrantFiefToClan
        {
            get { return this._canGrantFiefToClan; }

            set
            {
                if (value != this._canGrantFiefToClan)
                {
                    this._canGrantFiefToClan = value;
                    base.OnPropertyChanged("CanGrantFiefToClan");
                }
            }
        }

        [DataSourceProperty]
        public string GrantFiefActionName { get; }

        [DataSourceProperty]
        public HintViewModel GrantFiefHint
        {
            get
            {
                return this._grantFiefHint;
            }
            set
            {
                if (value != this._grantFiefHint)
                {
                    this._grantFiefHint = value;
                    base.OnPropertyChanged("GrantFiefHint");
                }
            }
        }

        [DataSourceProperty]
        public string GrantFiefExplanationText { get; }
        [DataSourceProperty]
        public string DonateGoldActionName { get; }

        [DataSourceProperty]
        public int DonationAmount { get; } = 1000;
    }
}
