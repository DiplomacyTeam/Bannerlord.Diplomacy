using System;
using System.ComponentModel;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
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
            _clan = clan;
            _onFinalize = onFinalize;
            PropertyChanged += HandlePropertyChanged;
            Refresh();

            AcceptText = new TextObject(StringConstants.Accept).ToString();
            CancelText = GameTexts.FindText("str_cancel", null).ToString();
            TitleText = new TextObject("{=Gzq6VHPt}Donate Gold").ToString();
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
            MaxValue = Clan.PlayerClan.Gold;
            GoldCost = new TextObject("{=e7uxH1jc}Gold Cost: {GOLD_COST}").SetTextVariable("GOLD_COST", IntValue).ToString();
            RelationGain = new TextObject("{=4lG3JG2e}Relation Gain: {RELATION_GAIN}+").SetTextVariable("RELATION_GAIN", GetEstimatedRelationValue()).ToString();
        }

        private void ExecutePropose()
        {
            GiveGoldToClanAction.ApplyFromHeroToClan(Hero.MainHero, _clan, IntValue);
            ChangeRelationAction.ApplyPlayerRelation(_clan.Leader, GetBaseRelationValueOfCurrentGoldCost());
            _onFinalize();
        }

        private int GetBaseRelationValueOfCurrentGoldCost()
        {
            if (_clan == Clan.PlayerClan)
            {
                return 0;
            }

            var influenceValue = IntValue * Campaign.Current.Models.DiplomacyModel.DenarsToInfluence();
            var relationValuePerInfluence = (float)Campaign.Current.Models.DiplomacyModel.GetRelationValueOfSupportingClan() / Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfSupportingClan();

            return MBMath.Round(influenceValue * relationValuePerInfluence);
        }

        private int GetEstimatedRelationValue()
        {
            var explainedNumber = new ExplainedNumber((float)GetBaseRelationValueOfCurrentGoldCost(), new StatExplainer(), null);
            Campaign.Current.Models.DiplomacyModel.GetRelationIncreaseFactor(Hero.MainHero, _clan.Leader, ref explainedNumber);
            return MBMath.Floor(explainedNumber.ResultNumber);
        }

        private void ExecuteCancel()
        {
            _onFinalize();
        }

        private void ExecuteReset()
        {
            IntValue = 0;
        }

        [DataSourceProperty]
        public float MaxValue
        {
            get { return _maxValue; }

            set
            {
                if (value != _maxValue)
                {
                    _maxValue = value;
                    base.OnPropertyChanged("MaxValue");
                }
            }
        }
        [DataSourceProperty]
        public string GoldCost
        {
            get { return _goldCost; }

            set
            {
                if (value != _goldCost)
                {
                    _goldCost = value;
                    base.OnPropertyChanged("GoldCost");
                }
            }
        }
        [DataSourceProperty]
        public string RelationGain
        {
            get { return _relationGain; }

            set
            {
                if (value != _relationGain)
                {
                    _relationGain = value;
                    base.OnPropertyChanged("RelationGain");
                }
            }
        }
        [DataSourceProperty]
        public float MinValue { get; } = 0f;

        [DataSourceProperty]
        public int IntValue
        {
            get { return _intValue; }

            set
            {
                if (value != _intValue)
                {
                    _intValue = value;
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
