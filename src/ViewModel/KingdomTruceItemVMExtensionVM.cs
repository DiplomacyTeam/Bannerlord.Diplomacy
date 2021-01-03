using Diplomacy.Costs;
using Diplomacy.DiplomaticAction.Alliance;
using Diplomacy.DiplomaticAction.NonAggressionPact;
using Diplomacy.DiplomaticAction.WarPeace;
using Diplomacy.Messengers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.KingdomDiplomacy;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Diplomacy.ViewModel
{
    public class KingdomTruceItemVMExtensionVM : KingdomTruceItemVM
    {
        public KingdomTruceItemVMExtensionVM(IFaction faction1, IFaction faction2, Action<KingdomDiplomacyItemVM> onSelection, Action<KingdomTruceItemVM> onAction) : base(faction1, faction2, onSelection, onAction)
        {
            SendMessengerActionName = new TextObject("{=cXfcwzPp}Send Messenger").ToString();
            AllianceActionName = new TextObject("{=0WPWbx70}Form Alliance").ToString();
            InfluenceCost = (int)DiplomacyCostCalculator.DetermineCostForDeclaringWar(Faction1 as Kingdom, true).Value;
            ActionName = GameTexts.FindText("str_kingdom_declate_war_action", null).ToString();
            NonAggressionPactActionName = new TextObject("{=9pY0NQrk}Form Pact").ToString();

            var textObject = new TextObject("{=9zlQNtlX}Form a non-aggression pact lasting {PACT_DURATION_DAYS} days.");
            textObject.SetTextVariable("PACT_DURATION_DAYS", Settings.Instance.NonAggressionPactDuration);
            NonAggressionPactHelpText = textObject.ToString();

            AllianceText = new TextObject("{=zpNalMeA}Alliances").ToString();
            WarsText = new TextObject("{=y5tXjbLK}Wars").ToString();
            PactsText = new TextObject(StringConstants.NonAggressionPacts).ToString();
            UpdateDiplomacyProperties();
        }

        protected override void UpdateDiplomacyProperties()
        {
            if (DiplomacyProperties is null)
            {
                DiplomacyProperties = new DiplomacyPropertiesVM(Faction1, Faction2);
            }
            DiplomacyProperties.UpdateDiplomacyProperties();

            base.UpdateDiplomacyProperties();
            UpdateActionAvailability();


            if (Settings.Instance.EnableWarExhaustion)
            {
                Stats.Insert(1, new KingdomWarComparableStatVM(
                    (int)Math.Ceiling(WarExhaustionManager.Instance.GetWarExhaustion((Kingdom)Faction1, (Kingdom)Faction2)),
                    (int)Math.Ceiling(WarExhaustionManager.Instance.GetWarExhaustion((Kingdom)Faction2, (Kingdom)Faction1)),
                    new TextObject("{=XmVTQ0bH}War Exhaustion"), _faction1Color, _faction2Color, 100, null));
            }
        }

        protected virtual void ExecuteExecutiveAction()
        {
            DiplomacyCostCalculator.DetermineCostForDeclaringWar(Faction1 as Kingdom, true).ApplyCost();
            DeclareWarAction.Apply(Faction1, Faction2);
        }


        protected virtual void UpdateActionAvailability()
        {
            IsMessengerAvailable = MessengerManager.CanSendMessengerWithCost(Faction2Leader.Hero, DiplomacyCostCalculator.DetermineCostForSendingMessenger());
            IsOptionAvailable = DeclareWarConditions.Instance.CanApplyExceptions(this, true).IsEmpty();
            var allianceException = FormAllianceConditions.Instance.CanApplyExceptions(this, true).FirstOrDefault()?.ToString();
            IsAllianceAvailable = allianceException is null;
            var declareWarException = DeclareWarConditions.Instance.CanApplyExceptions(this, true).FirstOrDefault()?.ToString();
            ActionHint = declareWarException is not null ? new HintViewModel(declareWarException) : new HintViewModel();
            AllianceHint = allianceException is not null ? new HintViewModel(allianceException) : new HintViewModel();
            var nonAggressionPactException = NonAggressionPactConditions.Instance.CanApplyExceptions(this, true).FirstOrDefault()?.ToString();
            IsNonAggressionPactAvailable = nonAggressionPactException is null;
            NonAggressionPactHint = nonAggressionPactException is not null ? new HintViewModel(nonAggressionPactException) : new HintViewModel();

            var allianceCost = DiplomacyCostCalculator.DetermineCostForFormingAlliance(Faction1 as Kingdom, Faction2 as Kingdom, true);
            AllianceInfluenceCost = (int)allianceCost.InfluenceCost.Value;
            AllianceGoldCost = (int)allianceCost.GoldCost.Value;


            var nonAggressionPactCost = DiplomacyCostCalculator.DetermineCostForFormingNonAggressionPact(Faction1 as Kingdom, Faction2 as Kingdom, true);
            NonAggressionPactInfluenceCost = (int)nonAggressionPactCost.InfluenceCost.Value;
            NonAggressionPactGoldCost = (int)nonAggressionPactCost.GoldCost.Value;


            AllianceScoreHint = UpdateDiplomacyTooltip(AllianceScoringModel.Instance.GetScore(Faction2 as Kingdom, Faction1 as Kingdom, new StatExplainer()));
            NonAggressionPactScoreHint = UpdateDiplomacyTooltip(NonAggressionPactScoringModel.Instance.GetScore(Faction2 as Kingdom, Faction1 as Kingdom, new StatExplainer()));
        }

        private static readonly TextObject _plusStr = new TextObject("{=eTw2aNV5}+", null);
        private static readonly TextObject _changeStr = new TextObject("{=XIBUWDlT}Required Score", null);
        private BasicTooltipViewModel UpdateDiplomacyTooltip(ExplainedNumber explainedNumber)
        {
            var list = new List<TooltipProperty>();
            {
                var value = string.Format("{0:0.##}", explainedNumber.ResultNumber);
                list.Add(new TooltipProperty(new TextObject("{=5r6fsHgm}Current Score").ToString(), value, 0, false, TooltipProperty.TooltipPropertyFlags.Title));
            }
            if (explainedNumber.Explainer.Lines.Count > 0)
            {
                foreach (var explanationLine in explainedNumber.Explainer.Lines)
                {
                    var value = string.Format("{0}{1:0.##}", (explanationLine.Number > 0.001f) ? _plusStr.ToString() : "", explanationLine.Number);
                    list.Add(new TooltipProperty(explanationLine.Name, value, 0, false, TooltipProperty.TooltipPropertyFlags.None));
                }
            }
            list.Add(new TooltipProperty("", string.Empty, 0, false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
            {
                var changeValue = explainedNumber.ResultNumber;
                var value = string.Format("{0:0.##}", AllianceScoringModel.Instance.ScoreThreshold);
                list.Add(new TooltipProperty(_changeStr.ToString(), value, 0, false, TooltipProperty.TooltipPropertyFlags.RundownResult));
            }
            return new BasicTooltipViewModel(() => list);
        }

        protected void SendMessenger()
        {
            Events.Instance.OnMessengerSent(Faction2Leader.Hero);
            UpdateDiplomacyProperties();
        }

        protected void FormAlliance()
        {
            DeclareAllianceAction.Apply(Faction1 as Kingdom, Faction2 as Kingdom, true);
        }

        protected void ProposeNonAggressionPact()
        {
            FormNonAggressionPactAction.Apply(Faction1 as Kingdom, Faction2 as Kingdom, true);
            UpdateDiplomacyProperties();
        }

        [DataSourceProperty]
        public bool IsAllianceAvailable
        {
            get
            {
                return _isAllianceAvailable;
            }
            set
            {
                if (value != _isAllianceAvailable)
                {
                    _isAllianceAvailable = value;
                    OnPropertyChanged("isAllianceAvailable");
                }
            }
        }

        [DataSourceProperty]
        public int AllianceInfluenceCost
        {
            get
            {
                return _allianceInfluenceCost;
            }
            set
            {
                if (value != _allianceInfluenceCost)
                {
                    _allianceInfluenceCost = value;
                    OnPropertyChanged("AllianceInfluenceCost");
                }
            }
        }

        [DataSourceProperty]
        public int AllianceGoldCost
        {
            get
            {
                return _allianceGoldCost;
            }
            set
            {
                if (value != _allianceGoldCost)
                {
                    _allianceGoldCost = value;
                    OnPropertyChanged("AllianceInfluenceCost");
                }
            }
        }

        [DataSourceProperty]
        public bool IsNonAggressionPactAvailable
        {
            get
            {
                return _isNonAggressionPactAvailable;
            }
            set
            {
                if (value != _isNonAggressionPactAvailable)
                {
                    _isNonAggressionPactAvailable = value;
                    OnPropertyChanged("IsNonAggressionPactAvailable");
                }
            }
        }

        [DataSourceProperty]
        public int NonAggressionPactInfluenceCost
        {
            get
            {
                return _nonAggressionPactInfluenceCost;
            }
            set
            {
                if (value != _nonAggressionPactInfluenceCost)
                {
                    _nonAggressionPactInfluenceCost = value;
                    OnPropertyChanged("NonAggressionPactInfluenceCost");
                }
            }
        }

        [DataSourceProperty]
        public int NonAggressionPactGoldCost
        {
            get
            {
                return _nonAggressionPactGoldCost;
            }
            set
            {
                if (value != _nonAggressionPactGoldCost)
                {
                    _nonAggressionPactGoldCost = value;
                    OnPropertyChanged("NonAggressionPactGoldCost");
                }
            }
        }

        [DataSourceProperty]
        public string ActionName { get; protected set; }

        [DataSourceProperty]
        public string NonAggressionPactActionName { get; }
        [DataSourceProperty]
        public string AllianceText { get; }
        [DataSourceProperty]
        public string WarsText { get; }
        [DataSourceProperty]
        public string PactsText { get; }
        [DataSourceProperty]
        public int SendMessengerGoldCost { get; } = (int)DiplomacyCostCalculator.DetermineCostForSendingMessenger().Value;

        [DataSourceProperty]
        public bool IsMessengerAvailable
        {
            get
            {
                return _isMessengerAvailable;
            }
            set
            {
                if (value != _isMessengerAvailable)
                {
                    _isMessengerAvailable = value;
                    OnPropertyChanged("IsMessengerAvailable");
                }
            }
        }

        [DataSourceProperty]
        public int InfluenceCost { get; protected set; }

        [DataSourceProperty]
        public bool IsOptionAvailable
        {
            get
            {
                return _isOptionAvailable;
            }
            set
            {
                if (value != _isOptionAvailable)
                {
                    _isOptionAvailable = value;
                    OnPropertyChanged("IsOptionAvailable");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel ActionHint
        {
            get
            {
                return _actionHint;
            }
            set
            {
                if (value != _actionHint)
                {
                    _actionHint = value;
                    OnPropertyChanged("ActionHint");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel AllianceHint
        {
            get
            {
                return _allianceHint;
            }
            set
            {
                if (value != _allianceHint)
                {
                    _allianceHint = value;
                    OnPropertyChanged("AllianceHint");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel NonAggressionPactHint
        {
            get
            {
                return _nonAggressionPactHint;
            }
            set
            {
                if (value != _nonAggressionPactHint)
                {
                    _nonAggressionPactHint = value;
                    OnPropertyChanged("NonAggressionPactHint");
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel AllianceScoreHint
        {
            get
            {
                return _allianceScoreHint;
            }
            set
            {
                if (value != _allianceScoreHint)
                {
                    _allianceScoreHint = value;
                    OnPropertyChanged("AllianceScoreHint");
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel NonAggressionPactScoreHint
        {
            get
            {
                return _nonAggressionPactScoreHint;
            }
            set
            {
                if (value != _nonAggressionPactScoreHint)
                {
                    _nonAggressionPactScoreHint = value;
                    OnPropertyChanged("NonAggressionPactScoreHint");
                }
            }
        }

        [DataSourceProperty]
        public string SendMessengerActionName { get; private set; }
        [DataSourceProperty]
        public string AllianceActionName { get; }
        [DataSourceProperty]
        public bool IsGoldCostVisible { get; } = false;

        [DataSourceProperty]
        public ImageIdentifierVM Faction1Image { get; private set; }

        [DataSourceProperty]
        public ImageIdentifierVM Faction2Image { get; private set; }
        [DataSourceProperty]
        public string NonAggressionPactHelpText { get; }
        [DataSourceProperty]
        public DiplomacyPropertiesVM DiplomacyProperties { get; private set; }

        private bool _isOptionAvailable;
        private bool _isMessengerAvailable;
        private bool _isAllianceAvailable;
        private bool _isNonAggressionPactAvailable;
        private int _allianceInfluenceCost;
        private int _nonAggressionPactInfluenceCost;
        private HintViewModel _allianceHint;
        private HintViewModel _nonAggressionPactHint;
        private HintViewModel _actionHint;
        private int _nonAggressionPactGoldCost;
        private BasicTooltipViewModel _allianceScoreHint;
        private int _allianceGoldCost;
        private BasicTooltipViewModel _nonAggressionPactScoreHint;
    }
}
