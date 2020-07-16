using System;
using System.ComponentModel;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.ViewModel
{
    internal class DonateGoldVM : TaleWorlds.Library.ViewModel
    {
        private Clan _clan;
        private Action _onFinalize;
        private float _maxValue;
        private int _intValue;
        private string _relationGain;
        private string _goldCost;

        public DonateGoldVM(Clan clan, Action onFinalize)
        {
            this._clan = clan;
            this._onFinalize = onFinalize;
            this.PropertyChanged += HandlePropertyChanged;
            this.Refresh();

            this.AcceptText = new TextObject(StringConstants.Accept).ToString();
            this.CancelText = GameTexts.FindText("str_cancel", null).ToString();
            this.TitleText = new TextObject("{=Gzq6VHPt}Donate Gold").ToString();
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (new string[] { "IntValue" }.Contains(e.PropertyName))
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            this.MaxValue = Clan.PlayerClan.Gold;
            this.GoldCost = new TextObject("{=e7uxH1jc}Gold Cost: {GOLD_COST}").SetTextVariable("GOLD_COST", this.IntValue).ToString();
            this.RelationGain = new TextObject("{=4lG3JG2e}Relation Gain: {RELATION_GAIN}+").SetTextVariable("RELATION_GAIN", GetEstimatedRelationValue()).ToString();
        }

        private void ExecutePropose()
        {
            GiveGoldToClanAction.ApplyFromHeroToClan(Hero.MainHero, _clan, this.IntValue);
            ChangeRelationAction.ApplyPlayerRelation(_clan.Leader, GetBaseRelationValueOfCurrentGoldCost());
            this._onFinalize();
        }

        private int GetBaseRelationValueOfCurrentGoldCost()
        {
            if (_clan == Clan.PlayerClan)
            {
                return 0;
            }

            float influenceValue = this.IntValue * Campaign.Current.Models.DiplomacyModel.DenarsToInfluence();
            float relationValuePerInfluence =(float)Campaign.Current.Models.DiplomacyModel.GetRelationValueOfSupportingClan() / Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfSupportingClan();
            
            return MBMath.Round(influenceValue * relationValuePerInfluence);
        }

        private int GetEstimatedRelationValue()
        {
            ExplainedNumber explainedNumber = new ExplainedNumber((float)GetBaseRelationValueOfCurrentGoldCost(), new StatExplainer(), null);
            Campaign.Current.Models.DiplomacyModel.GetRelationIncreaseFactor(Hero.MainHero, _clan.Leader, ref explainedNumber);
            return MBMath.Floor(explainedNumber.ResultNumber);
        }

        private void ExecuteCancel()
        {
            this._onFinalize();
        }

        private void ExecuteReset()
        {
            this.IntValue = 0;
        }

        [DataSourceProperty]
        public float MaxValue
        {
            get { return this._maxValue; }

            set
            {
                if (value != this._maxValue)
                {
                    this._maxValue = value;
                    base.OnPropertyChanged("MaxValue");
                }
            }
        }
        [DataSourceProperty]
        public string GoldCost
        {
            get { return this._goldCost; }

            set
            {
                if (value != this._goldCost)
                {
                    this._goldCost = value;
                    base.OnPropertyChanged("GoldCost");
                }
            }
        }
        [DataSourceProperty]
        public string RelationGain
        {
            get { return this._relationGain; }

            set
            {
                if (value != this._relationGain)
                {
                    this._relationGain = value;
                    base.OnPropertyChanged("RelationGain");
                }
            }
        }
        [DataSourceProperty]
        public float MinValue { get; } = 0f;

        [DataSourceProperty]
        public int IntValue
        {
            get { return this._intValue; }

            set
            {
                if (value != this._intValue)
                {
                    this._intValue = value;
                    base.OnPropertyChanged("IntValue");
                }
            }
        }
        [DataSourceProperty]
        public string AcceptText { get; }
        [DataSourceProperty]
        public string CancelText { get; }
        [DataSourceProperty]
        public string TitleText { get; }
    }
}