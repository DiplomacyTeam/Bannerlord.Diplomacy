using DiplomacyFixes.Costs;
using DiplomacyFixes.DiplomaticAction.Alliance;
using DiplomacyFixes.DiplomaticAction.NonAggressionPact;
using DiplomacyFixes.DiplomaticAction.WarPeace;
using DiplomacyFixes.Messengers;
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

namespace DiplomacyFixes.ViewModel
{
    public class KingdomTruceItemVMExtensionVM : KingdomTruceItemVM
    {
        public KingdomTruceItemVMExtensionVM(IFaction faction1, IFaction faction2, Action<KingdomDiplomacyItemVM> onSelection, Action<KingdomTruceItemVM> onAction) : base(faction1, faction2, onSelection, onAction)
        {
            this.SendMessengerActionName = new TextObject("{=cXfcwzPp}Send Messenger").ToString();
            this.AllianceActionName = new TextObject("{=0WPWbx70}Form Alliance").ToString();
            this.InfluenceCost = (int)DiplomacyCostCalculator.DetermineCostForDeclaringWar(Faction1 as Kingdom, true).Value;
            this.ActionName = GameTexts.FindText("str_kingdom_declate_war_action", null).ToString();
            this.NonAggressionPactActionName = new TextObject("{=9pY0NQrk}Form Pact").ToString();

            TextObject textObject = new TextObject("{=9zlQNtlX}Form a non-aggression pact lasting {PACT_DURATION_DAYS} days.");
            textObject.SetTextVariable("PACT_DURATION_DAYS", Settings.Instance.NonAggressionPactDuration);
            this.NonAggressionPactHelpText = textObject.ToString();

            this.AllianceText = new TextObject("{=zpNalMeA}Alliances").ToString();
            this.WarsText = new TextObject("{=y5tXjbLK}Wars").ToString();
            this.PactsText = new TextObject(StringConstants.NonAggressionPacts).ToString();
            UpdateDiplomacyProperties();
        }

        protected override void UpdateDiplomacyProperties()
        {
            if (this.DiplomacyProperties == null)
            {
                this.DiplomacyProperties = new DiplomacyPropertiesVM(Faction1, Faction2);
            }
            this.DiplomacyProperties.UpdateDiplomacyProperties();

            base.UpdateDiplomacyProperties();
            UpdateActionAvailability();


            if (Settings.Instance.EnableWarExhaustion)
            {
                this.Stats.Insert(1, new KingdomWarComparableStatVM(
                    (int)Math.Ceiling(WarExhaustionManager.Instance.GetWarExhaustion((Kingdom)this.Faction1, (Kingdom)this.Faction2)),
                    (int)Math.Ceiling(WarExhaustionManager.Instance.GetWarExhaustion((Kingdom)this.Faction2, (Kingdom)this.Faction1)),
                    new TextObject("{=XmVTQ0bH}War Exhaustion"), this._faction1Color, this._faction2Color, 100, null));
            }
        }

        protected virtual void ExecuteExecutiveAction()
        {
            DiplomacyCostCalculator.DetermineCostForDeclaringWar(Faction1 as Kingdom, true).ApplyCost();
            DeclareWarAction.Apply(Faction1, Faction2);
        }


        protected virtual void UpdateActionAvailability()
        {
            this.IsMessengerAvailable = MessengerManager.CanSendMessengerWithCost(Faction2Leader.Hero, DiplomacyCostCalculator.DetermineCostForSendingMessenger());
            this.IsOptionAvailable = DeclareWarConditions.Instance.CanApplyExceptions(this, true).IsEmpty();
            string allianceException = FormAllianceConditions.Instance.CanApplyExceptions(this, true).FirstOrDefault()?.ToString();
            this.IsAllianceAvailable = allianceException == null;
            string declareWarException = DeclareWarConditions.Instance.CanApplyExceptions(this, true).FirstOrDefault()?.ToString();
            this.ActionHint = declareWarException != null ? new HintViewModel(declareWarException) : new HintViewModel();
            this.AllianceHint = allianceException != null ? new HintViewModel(allianceException) : new HintViewModel();
            string nonAggressionPactException = NonAggressionPactConditions.Instance.CanApplyExceptions(this, true).FirstOrDefault()?.ToString();
            this.IsNonAggressionPactAvailable = nonAggressionPactException == null;
            this.NonAggressionPactHint = nonAggressionPactException != null ? new HintViewModel(nonAggressionPactException) : new HintViewModel();

            HybridCost allianceCost = DiplomacyCostCalculator.DetermineCostForFormingAlliance(Faction1 as Kingdom, Faction2 as Kingdom, true);
            this.AllianceInfluenceCost = (int)allianceCost.InfluenceCost.Value;
            this.AllianceGoldCost = (int)allianceCost.GoldCost.Value;


            HybridCost nonAggressionPactCost = DiplomacyCostCalculator.DetermineCostForFormingNonAggressionPact(Faction1 as Kingdom, Faction2 as Kingdom, true);
            this.NonAggressionPactInfluenceCost = (int)nonAggressionPactCost.InfluenceCost.Value;
            this.NonAggressionPactGoldCost = (int)nonAggressionPactCost.GoldCost.Value;


            this.AllianceScoreHint = this.UpdateDiplomacyTooltip(AllianceScoringModel.Instance.GetScore(Faction2 as Kingdom, Faction1 as Kingdom, new StatExplainer()));
            this.NonAggressionPactScoreHint = this.UpdateDiplomacyTooltip(NonAggressionPactScoringModel.Instance.GetScore(Faction2 as Kingdom, Faction1 as Kingdom, new StatExplainer()));
        }

        private static readonly TextObject _plusStr = new TextObject("{=eTw2aNV5}+", null);
        private static readonly TextObject _changeStr = new TextObject("{=XIBUWDlT}Required Score", null);
        private BasicTooltipViewModel UpdateDiplomacyTooltip(ExplainedNumber explainedNumber)
        {
            List<TooltipProperty> list = new List<TooltipProperty>();
            {
                string value = string.Format("{0:0.##}", explainedNumber.ResultNumber);
                list.Add(new TooltipProperty(new TextObject("{=5r6fsHgm}Current Score").ToString(), value, 0, false, TooltipProperty.TooltipPropertyFlags.Title));
            }
            if (explainedNumber.Explainer.Lines.Count > 0)
            {
                foreach (StatExplainer.ExplanationLine explanationLine in explainedNumber.Explainer.Lines)
                {
                    string value = string.Format("{0}{1:0.##}", (explanationLine.Number > 0.001f) ? _plusStr.ToString() : "", explanationLine.Number);
                    list.Add(new TooltipProperty(explanationLine.Name, value, 0, false, TooltipProperty.TooltipPropertyFlags.None));
                }
            }
            list.Add(new TooltipProperty("", string.Empty, 0, false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
            {
                float changeValue = explainedNumber.ResultNumber;
                string value = string.Format("{0:0.##}", AllianceScoringModel.Instance.ScoreThreshold);
                list.Add(new TooltipProperty(_changeStr.ToString(), value, 0, false, TooltipProperty.TooltipPropertyFlags.RundownResult));
            }
            return new BasicTooltipViewModel(() => list);
        }

        protected void SendMessenger()
        {
            Events.Instance.OnMessengerSent(Faction2Leader.Hero);
            this.UpdateDiplomacyProperties();
        }

        protected void FormAlliance()
        {
            DeclareAllianceAction.Apply(this.Faction1 as Kingdom, this.Faction2 as Kingdom, true);
        }

        protected void ProposeNonAggressionPact()
        {
            FormNonAggressionPactAction.Apply(this.Faction1 as Kingdom, this.Faction2 as Kingdom, true);
            this.UpdateDiplomacyProperties();
        }

        [DataSourceProperty]
        public bool IsAllianceAvailable
        {
            get
            {
                return this._isAllianceAvailable;
            }
            set
            {
                if (value != this._isAllianceAvailable)
                {
                    this._isAllianceAvailable = value;
                    base.OnPropertyChanged("isAllianceAvailable");
                }
            }
        }

        [DataSourceProperty]
        public int AllianceInfluenceCost
        {
            get
            {
                return this._allianceInfluenceCost;
            }
            set
            {
                if (value != this._allianceInfluenceCost)
                {
                    this._allianceInfluenceCost = value;
                    base.OnPropertyChanged("AllianceInfluenceCost");
                }
            }
        }

        [DataSourceProperty]
        public int AllianceGoldCost
        {
            get
            {
                return this._allianceGoldCost;
            }
            set
            {
                if (value != this._allianceGoldCost)
                {
                    this._allianceGoldCost = value;
                    base.OnPropertyChanged("AllianceInfluenceCost");
                }
            }
        }

        [DataSourceProperty]
        public bool IsNonAggressionPactAvailable
        {
            get
            {
                return this._isNonAggressionPactAvailable;
            }
            set
            {
                if (value != this._isNonAggressionPactAvailable)
                {
                    this._isNonAggressionPactAvailable = value;
                    base.OnPropertyChanged("IsNonAggressionPactAvailable");
                }
            }
        }

        [DataSourceProperty]
        public int NonAggressionPactInfluenceCost
        {
            get
            {
                return this._nonAggressionPactInfluenceCost;
            }
            set
            {
                if (value != this._nonAggressionPactInfluenceCost)
                {
                    this._nonAggressionPactInfluenceCost = value;
                    base.OnPropertyChanged("NonAggressionPactInfluenceCost");
                }
            }
        }

        [DataSourceProperty]
        public int NonAggressionPactGoldCost
        {
            get
            {
                return this._nonAggressionPactGoldCost;
            }
            set
            {
                if (value != this._nonAggressionPactGoldCost)
                {
                    this._nonAggressionPactGoldCost = value;
                    base.OnPropertyChanged("NonAggressionPactGoldCost");
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
                return this._isMessengerAvailable;
            }
            set
            {
                if (value != this._isMessengerAvailable)
                {
                    this._isMessengerAvailable = value;
                    base.OnPropertyChanged("IsMessengerAvailable");
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
                return this._isOptionAvailable;
            }
            set
            {
                if (value != this._isOptionAvailable)
                {
                    this._isOptionAvailable = value;
                    base.OnPropertyChanged("IsOptionAvailable");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel ActionHint
        {
            get
            {
                return this._actionHint;
            }
            set
            {
                if (value != this._actionHint)
                {
                    this._actionHint = value;
                    base.OnPropertyChanged("ActionHint");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel AllianceHint
        {
            get
            {
                return this._allianceHint;
            }
            set
            {
                if (value != this._allianceHint)
                {
                    this._allianceHint = value;
                    base.OnPropertyChanged("AllianceHint");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel NonAggressionPactHint
        {
            get
            {
                return this._nonAggressionPactHint;
            }
            set
            {
                if (value != this._nonAggressionPactHint)
                {
                    this._nonAggressionPactHint = value;
                    base.OnPropertyChanged("NonAggressionPactHint");
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel AllianceScoreHint
        {
            get
            {
                return this._allianceScoreHint;
            }
            set
            {
                if (value != this._allianceScoreHint)
                {
                    this._allianceScoreHint = value;
                    base.OnPropertyChanged("AllianceScoreHint");
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel NonAggressionPactScoreHint
        {
            get
            {
                return this._nonAggressionPactScoreHint;
            }
            set
            {
                if (value != this._nonAggressionPactScoreHint)
                {
                    this._nonAggressionPactScoreHint = value;
                    base.OnPropertyChanged("NonAggressionPactScoreHint");
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
