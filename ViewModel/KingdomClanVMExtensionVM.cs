using DiplomacyFixes.GauntletInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomClan;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.ViewModel
{
    class KingdomClanVMExtensionVM : KingdomClanVM
    {
        private bool _canGrantFiefToClan;
        private GrantFiefInterface _grantFiefInterface;
        private string _grantFiefActionName;

        public KingdomClanVMExtensionVM(Action<TaleWorlds.CampaignSystem.Election.KingdomDecision> forceDecide) : base(forceDecide) {
            Events.FiefGranted.AddNonSerializedListener(this, this.RefreshCanGrantFief);
            this.CanGrantFiefToClan = Clan.PlayerClan.Fortifications.Count > 1;
            this._grantFiefInterface = new GrantFiefInterface();
            this.GrantFiefActionName = new TextObject("{=LpoyhORp}Grant Fief").ToString();
        }

        public void GrantFief()
        {
            this._grantFiefInterface.ShowFiefInterface(ScreenManager.TopScreen, this.CurrentSelectedClan.Clan.Leader);
        }

        private void RefreshCanGrantFief(Town town)
        {
            this.CanGrantFiefToClan = Clan.PlayerClan.Fortifications.Count > 1;
            base.RefreshClan();
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
        public string GrantFiefActionName
        {
            get { return this._grantFiefActionName; }

            set
            {
                if (value != this._grantFiefActionName)
                {
                    this._grantFiefActionName = value;
                    base.OnPropertyChanged("GrantFiefActionName");
                }
            }
        }
    }
}
