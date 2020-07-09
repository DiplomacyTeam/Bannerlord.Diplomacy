using DiplomacyFixes.DiplomaticAction.Usurp;
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
        private bool _canUsurpThrone;
        private bool _showUsurpThrone;
        private int _usurpThroneInfluenceCost;
        private string _usurpThroneExplanationText;

        public KingdomClanVMExtensionVM(Action<TaleWorlds.CampaignSystem.Election.KingdomDecision> forceDecide) : base(forceDecide)
        {
            this._executeExpel = () => typeof(KingdomClanVM).GetMethod("ExecuteExpelCurrentClan", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { });
            this._executeSupport = () => typeof(KingdomClanVM).GetMethod("ExecuteSupport", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { });

            Events.FiefGranted.AddNonSerializedListener(this, this.RefreshCanGrantFief);
            this._grantFiefInterface = new GrantFiefInterface();
            this.GrantFiefActionName = new TextObject("{=LpoyhORp}Grant Fief").ToString();
            this.GrantFiefExplanationText = new TextObject("{=98hwXUTp}Grant fiefs to clans in your kingdom").ToString();
            this.DonateGoldActionName = new TextObject("{=Gzq6VHPt}Donate Gold").ToString();
            this.UsurpThroneActionName = new TextObject("{=N7goPgiq}Usurp Throne").ToString();
            base.PropertyChanged += new PropertyChangedEventHandler(this.OnPropertyChanged);
            RefreshCanGrantFief();
            RefreshCanDonateGold();
            RefreshCanUsurpThrone();
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
                RefreshCanDonateGold();
                RefreshCanUsurpThrone();
            }
        }

        private void RefreshCanUsurpThrone()
        {
            this.ShowUsurpThrone = CurrentSelectedClan.Clan == Clan.PlayerClan;
            this.CanUsurpThrone = UsurpKingdomAction.CanUsurp(Clan.PlayerClan);
            UsurpKingdomAction.GetClanSupport(Clan.PlayerClan, out int supportingClanTiers, out int opposingClanTiers);

            TextObject textObject = new TextObject("{=WVe7QwhW}Usurp the throne of this kingdom\nClan Support: {SUPPORTING_TIERS} / {OPPOSING_TIERS}");
            textObject.SetTextVariable("SUPPORTING_TIERS", supportingClanTiers);
            textObject.SetTextVariable("OPPOSING_TIERS", opposingClanTiers + 1);
            this.UsurpThroneExplanationText = textObject.ToString();
            this.UsurpThroneInfluenceCost = (int)UsurpKingdomAction.GetUsurpInfluenceCost(Clan.PlayerClan);
        }

        public void UsurpThrone()
        {
            UsurpKingdomAction.Apply(Clan.PlayerClan);
            RefreshClan();
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
            this.CanDonateGold = this.CurrentSelectedClan.Clan != Clan.PlayerClan && Hero.MainHero.Gold >= DonationAmount;
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
        public string UsurpThroneActionName { get; }
        [DataSourceProperty]
        public string UsurpThroneExplanationText
        {
            get { return this._usurpThroneExplanationText; }

            set
            {
                if (value != this._usurpThroneExplanationText)
                {
                    this._usurpThroneExplanationText = value;
                    base.OnPropertyChanged("UsurpThroneExplanationText");
                }
            }

        }
        [DataSourceProperty]
        public string DonateGoldActionName { get; }

        [DataSourceProperty]
        public int DonationAmount { get; } = 1000;
        [DataSourceProperty]
        public bool ShowUsurpThrone
        {
            get { return this._showUsurpThrone; }

            set
            {
                if (value != this._showUsurpThrone)
                {
                    this._showUsurpThrone = value;
                    base.OnPropertyChanged("ShowUsurpThrone");
                }
            }

        }

        [DataSourceProperty]
        public bool CanUsurpThrone
        {
            get { return this._canUsurpThrone; }

            set
            {
                if (value != this._canUsurpThrone)
                {
                    this._canUsurpThrone = value;
                    base.OnPropertyChanged("CanUsurpThrone");
                }
            }
        }

        [DataSourceProperty]
        public int UsurpThroneInfluenceCost
        {
            get { return this._usurpThroneInfluenceCost; }

            set
            {
                if (value != this._usurpThroneInfluenceCost)
                {
                    this._usurpThroneInfluenceCost = value;
                    base.OnPropertyChanged("UsurpThroneInfluenceCost");
                }
            }
        }
    }
}
