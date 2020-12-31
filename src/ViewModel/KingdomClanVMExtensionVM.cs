using Diplomacy.DiplomaticAction.Usurp;
using Diplomacy.GauntletInterfaces;
using Diplomacy.GrantFief;
using System;
using System.ComponentModel;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomClan;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    class KingdomClanVMExtensionVM : KingdomClanVM, INotifyPropertyChanged, ICloseableVM
    {
        private bool _canGrantFiefToClan;
        private GrantFiefInterface _grantFiefInterface;
        private HintViewModel _grantFiefHint;

        private Action _executeExpel;
        private Action _executeSupport;
        private bool _canUsurpThrone;
        private bool _showUsurpThrone;
        private int _usurpThroneInfluenceCost;
        private string _usurpThroneExplanationText;
        private HintViewModel _usurpThroneHint;

        public KingdomClanVMExtensionVM(Action<TaleWorlds.CampaignSystem.Election.KingdomDecision> forceDecide) : base(forceDecide)
        {
            this._executeExpel = () => typeof(KingdomClanVM).GetMethod("ExecuteExpelCurrentClan", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { });
            this._executeSupport = () => typeof(KingdomClanVM).GetMethod("ExecuteSupport", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, new object[] { });

            Events.FiefGranted.AddNonSerializedListener(this, this.RefreshCanGrantFief);
            this._grantFiefInterface = new GrantFiefInterface();
            this.GrantFiefActionName = new TextObject("{=LpoyhORp}Grant Fief").ToString();
            this.GrantFiefExplanationText = new TextObject("{=98hwXUTp}Grant fiefs to clans in your kingdom").ToString();
            this.DonateGoldActionName = new TextObject("{=Gzq6VHPt}Donate Gold").ToString();
            this.DonateGoldExplanationText = new TextObject("{=7QvXkcxH}Donate gold to clans in your kingdom").ToString();
            this.UsurpThroneActionName = new TextObject("{=N7goPgiq}Usurp Throne").ToString();
            base.PropertyChanged += new PropertyChangedEventHandler(this.OnPropertyChanged);
            base.PropertyChangedWithValue += new PropertyChangedWithValueEventHandler(this.OnPropertyChangedWithValue);
            RefreshCanGrantFief();
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

        // e1.4.2 compatible
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentSelectedClan")
            {
                RefreshCanGrantFief();
                RefreshCanUsurpThrone();
            }
        }

        // e1.4.3 compatible
        private void OnPropertyChangedWithValue(object sender, PropertyChangedWithValueEventArgs e)
        {
            if (e.PropertyName == "CurrentSelectedClan")
            {
                RefreshCanGrantFief();
                RefreshCanUsurpThrone();
            }
        }

        private void RefreshCanUsurpThrone()
        {
            this.ShowUsurpThrone = CurrentSelectedClan.Clan == Clan.PlayerClan;
            this.CanUsurpThrone = UsurpKingdomAction.CanUsurp(Clan.PlayerClan, out string errorMessage);
            this.UsurpThroneHint = errorMessage != null ? new HintViewModel(errorMessage) : new HintViewModel();
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
            new DonateGoldInterface().ShowInterface(ScreenManager.TopScreen, this.CurrentSelectedClan.Clan);
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
        public string DonateGoldExplanationText { get; }

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

        [DataSourceProperty]
        public HintViewModel UsurpThroneHint
        {
            get { return this._usurpThroneHint; }

            set
            {
                if (value != this._usurpThroneHint)
                {
                    this._usurpThroneHint = value;
                    base.OnPropertyChanged("UsurpThroneHint");
                }
            }
        }
    }
}
