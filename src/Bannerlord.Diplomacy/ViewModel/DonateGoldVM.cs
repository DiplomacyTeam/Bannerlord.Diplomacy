using Diplomacy.Actions;

using JetBrains.Annotations;

using System;
using System.ComponentModel;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    internal sealed class DonateGoldVM : TaleWorlds.Library.ViewModel
    {
        private static readonly TextObject _TGoldCost = new("{=e7uxH1jc}Gold Cost: {GOLD_COST}");
        private static readonly TextObject _TRelationGain = new("{=4lG3JG2e}Relation Gain: {RELATION_GAIN}+");
        private readonly Clan _clan;
        private readonly Action _onFinalize;
        private string _goldCost = string.Empty;
        private int _intValue;
        private float _maxValue;
        private string _relationGain = string.Empty;

        [DataSourceProperty]
        public float MaxValue
        {
            get => _maxValue;
            set
            {
                if (value != _maxValue)
                {
                    _maxValue = value;
                    OnPropertyChanged(nameof(MaxValue));
                }
            }
        }

        [DataSourceProperty]
        public string GoldCost
        {
            get => _goldCost;
            set
            {
                if (value != _goldCost)
                {
                    _goldCost = value;
                    OnPropertyChanged(nameof(GoldCost));
                }
            }
        }

        [DataSourceProperty]
        public string RelationGain
        {
            get => _relationGain;
            set
            {
                if (value != _relationGain)
                {
                    _relationGain = value;
                    OnPropertyChanged(nameof(RelationGain));
                }
            }
        }

        [DataSourceProperty][UsedImplicitly] public float MinValue { get; }

        [DataSourceProperty]
        public int IntValue
        {
            get => _intValue;
            set
            {
                if (value != _intValue)
                {
                    _intValue = value;
                    OnPropertyChanged(nameof(IntValue));
                }
            }
        }

        [DataSourceProperty][UsedImplicitly] public string AcceptText { get; } = new TextObject(StringConstants.Accept).ToString();

        [DataSourceProperty][UsedImplicitly] public string CancelText { get; } = GameTexts.FindText("str_cancel").ToString();

        [DataSourceProperty][UsedImplicitly] public string TitleText { get; } = new TextObject("{=Gzq6VHPt}Donate Gold").ToString();

        public DonateGoldVM(Clan clan, Action onFinalize)
        {
            _clan = clan;
            _onFinalize = onFinalize;
            PropertyChanged += HandlePropertyChanged;
            Refresh();
        }

        [UsedImplicitly]
        private void ExecuteCancel()
        {
            _onFinalize();
        }

        [UsedImplicitly]
        private void ExecuteReset()
        {
            IntValue = 0;
        }

        [UsedImplicitly]
        private void ExecutePropose()
        {
            GiveGoldToClanAction.ApplyFromHeroToClan(Hero.MainHero, _clan, IntValue);

            var relationValue = GetBaseRelationValueOfCurrentGoldCost();

            if (relationValue > 0)
                ChangeRelationAction.ApplyPlayerRelation(_clan.Leader, relationValue);

            _onFinalize();
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IntValue))
                Refresh();
        }

        private void Refresh()
        {
            MaxValue = Clan.PlayerClan.Gold;
            GoldCost = _TGoldCost.CopyTextObject().SetTextVariable("GOLD_COST", IntValue).ToString();
            RelationGain = _TRelationGain.CopyTextObject().SetTextVariable("RELATION_GAIN", GetEstimatedRelationValue()).ToString();
        }

        private int GetBaseRelationValueOfCurrentGoldCost()
        {
            if (_clan == Clan.PlayerClan)
                return 0;

            var influenceValue = IntValue * Campaign.Current.Models.DiplomacyModel.DenarsToInfluence();
            var relationValuePerInfluence = (float) Campaign.Current.Models.DiplomacyModel.GetRelationValueOfSupportingClan()
                                            / Campaign.Current.Models.DiplomacyModel.GetInfluenceCostOfSupportingClan();

            return (int) Math.Round(influenceValue * relationValuePerInfluence);
        }

        private int GetEstimatedRelationValue()
        {
            var baseRelation = GetBaseRelationValueOfCurrentGoldCost();
            var adjustedRelation = Campaign.Current.Models.DiplomacyModel.GetRelationIncreaseFactor(Hero.MainHero, _clan.Leader, baseRelation);
            return (int) Math.Floor(adjustedRelation);
        }
    }
}