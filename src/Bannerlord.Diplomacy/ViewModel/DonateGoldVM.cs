using Diplomacy.Actions;
using Diplomacy.Character;

using JetBrains.Annotations;

using System;
using System.ComponentModel;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
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
        public float MaxValue { get => _maxValue; set => SetField(ref _maxValue, value, nameof(MaxValue)); }

        [DataSourceProperty]
        public string GoldCost { get => _goldCost; set => SetField(ref _goldCost, value, nameof(GoldCost)); }

        [DataSourceProperty]
        public string RelationGain { get => _relationGain; set => SetField(ref _relationGain, value, nameof(RelationGain)); }

        [DataSourceProperty]
        [UsedImplicitly]
        public float MinValue { get; }

        [DataSourceProperty]
        public int IntValue { get => _intValue; set => SetField(ref _intValue, value, nameof(IntValue)); }

        [DataSourceProperty]
        [UsedImplicitly]
        public string AcceptText { get; } = new TextObject(StringConstants.Accept).ToString();

        [DataSourceProperty]
        [UsedImplicitly]
        public string CancelText { get; } = GameTexts.FindText("str_cancel").ToString();

        [DataSourceProperty]
        [UsedImplicitly]
        public string TitleText { get; } = new TextObject("{=Gzq6VHPt}Donate Gold").ToString();

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
            {
                ChangeRelationAction.ApplyPlayerRelation(_clan.Leader, relationValue);
                PlayerCharacterTraitHelper.UpdateTrait(DefaultTraits.Generosity, MBMath.ClampInt(relationValue * 5, 0, 50));
                PlayerCharacterTraitHelper.UpdateTrait(DefaultTraits.Calculating, MBMath.ClampInt(relationValue * GetCalculatingTraitFactor(), 0, 50));
            }

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

        private int GetCalculatingTraitFactor() => Math.Max(70 - (int) _clan.Leader.GetRelationWithPlayer(), 0) / 20 * (_clan.Tier / 2);
    }
}