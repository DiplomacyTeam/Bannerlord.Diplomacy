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
    internal class KingdomTruceItemVMExtensionVM : KingdomTruceItemVM
    {
        private static readonly string _SSendMessenger = new TextObject("{=cXfcwzPp}Send Messenger").ToString();
        private static readonly string _SFormAlliance = new TextObject("{=0WPWbx70}Form Alliance").ToString();
        private static readonly string _SFormPact = new TextObject("{=9pY0NQrk}Form Pact").ToString();
        private static readonly string _SWars = new TextObject("{=y5tXjbLK}Wars").ToString();
        private static readonly string _SAlliances = new TextObject("{=zpNalMeA}Alliances").ToString();
        private static readonly string _SPacts = new TextObject(StringConstants.NonAggressionPacts).ToString();

        private static readonly TextObject _TNapHelpText = new("{=9zlQNtlX}Form a non-aggression pact lasting {DAYS} days.");

        public KingdomTruceItemVMExtensionVM(IFaction faction1,
                                             IFaction faction2,
                                             Action<KingdomDiplomacyItemVM> onSelection,
                                             Action<KingdomTruceItemVM> onAction)
            : base(faction1, faction2, onSelection, onAction)
        {
            SendMessengerActionName = _SSendMessenger;
            AllianceActionName = _SFormAlliance;
            InfluenceCost = (int)DiplomacyCostCalculator.DetermineCostForDeclaringWar((Kingdom)Faction1, true).Value;
            ActionName = GameTexts.FindText("str_kingdom_declate_war_action", null).ToString();
            NonAggressionPactActionName = _SFormPact;
            NonAggressionPactHelpText = _TNapHelpText.SetTextVariable("DAYS", Settings.Instance!.NonAggressionPactDuration).ToString();
            AllianceText = _SAlliances;
            WarsText = _SWars;
            PactsText = _SPacts;

            UpdateDiplomacyProperties();
        }

        private static readonly TextObject _TWarExhaustion = new("{=XmV_TQ0bH}War Exhaustion");

        protected override void UpdateDiplomacyProperties()
        {
            if (DiplomacyProperties is null)
                DiplomacyProperties = new DiplomacyPropertiesVM(Faction1, Faction2);

            DiplomacyProperties.UpdateDiplomacyProperties();
            base.UpdateDiplomacyProperties();
            UpdateActionAvailability();

            if (Settings.Instance!.EnableWarExhaustion)
            {
                float warExhaustion1 = WarExhaustionManager.Instance.GetWarExhaustion((Kingdom)Faction1, (Kingdom)Faction2);
                float warExhaustion2 = WarExhaustionManager.Instance.GetWarExhaustion((Kingdom)Faction2, (Kingdom)Faction1);

                Stats.Insert(1, new KingdomWarComparableStatVM(MBMath.Ceiling(warExhaustion1),
                                                               MBMath.Ceiling(warExhaustion2),
                                                               _TWarExhaustion,
                                                               _faction1Color,
                                                               _faction2Color,
                                                               100,
                                                               null));
            }
        }

        protected virtual void UpdateActionAvailability()
        {
            IsMessengerAvailable = MessengerManager.CanSendMessengerWithCost(Faction2Leader.Hero,
                                                                             DiplomacyCostCalculator.DetermineCostForSendingMessenger());

            IsOptionAvailable = DeclareWarConditions.Instance.CanApplyExceptions(this, true).IsEmpty();

            var allianceException = FormAllianceConditions.Instance.CanApplyExceptions(this, true).FirstOrDefault()?.ToString();
            var declareWarException = DeclareWarConditions.Instance.CanApplyExceptions(this, true).FirstOrDefault()?.ToString();
            var napException = NonAggressionPactConditions.Instance.CanApplyExceptions(this, true).FirstOrDefault()?.ToString();

            IsAllianceAvailable = allianceException is null;
            IsNonAggressionPactAvailable = napException is null;

            ActionHint = declareWarException is not null ? new HintViewModel(declareWarException) : new HintViewModel();
            AllianceHint = allianceException is not null ? new HintViewModel(allianceException) : new HintViewModel();
            NonAggressionPactHint = napException is not null ? new HintViewModel(napException) : new HintViewModel();

            var allianceCost = DiplomacyCostCalculator.DetermineCostForFormingAlliance((Kingdom)Faction1,
                                                                                       (Kingdom)Faction2,
                                                                                       true);
            AllianceInfluenceCost = (int)allianceCost.InfluenceCost.Value;
            AllianceGoldCost = (int)allianceCost.GoldCost.Value;

            var nonAggressionPactCost = DiplomacyCostCalculator.DetermineCostForFormingNonAggressionPact((Kingdom)Faction1,
                                                                                                         (Kingdom)Faction2,
                                                                                                         true);
            NonAggressionPactInfluenceCost = (int)nonAggressionPactCost.InfluenceCost.Value;
            NonAggressionPactGoldCost = (int)nonAggressionPactCost.GoldCost.Value;

            var allianceScore = AllianceScoringModel.Instance.GetScore((Kingdom)Faction2, (Kingdom)Faction1, true);
            var napScore = NonAggressionPactScoringModel.Instance.GetScore((Kingdom)Faction2, (Kingdom)Faction1, true);
            AllianceScoreHint = UpdateDiplomacyTooltip(allianceScore);
            NonAggressionPactScoreHint = UpdateDiplomacyTooltip(napScore);
        }

        private static readonly string _SPlus = new TextObject("{=eTw2aNV5}+").ToString();
        private static readonly string _SRequiredScore = new TextObject("{=XIBUWDlT}Required Score").ToString();
        private static readonly string _SCurrentScore = new TextObject("{=5r6fsHgm}Current Score").ToString();

        private BasicTooltipViewModel UpdateDiplomacyTooltip(ExplainedNumber explainedNumber)
        {
            static string PlusPrefixed(float value) => $"{(value >= 0.005f ? _SPlus : string.Empty)}{value:0.##}";

            var list = new List<TooltipProperty>
            {
                new(_SCurrentScore, $"{explainedNumber.ResultNumber:0.##}", 0, false, TooltipProperty.TooltipPropertyFlags.Title)
            };

            // FIXME: Must test whether this e1.5.7 adaptation displays the base score!
            foreach (var (name, number) in explainedNumber.GetLines())
                list.Add(new(name, PlusPrefixed(number), 0, false, TooltipProperty.TooltipPropertyFlags.None));

            list.Add(new(string.Empty, string.Empty, 0, false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
            list.Add(new(_SRequiredScore, $"{AllianceScoringModel.Instance.ScoreThreshold:0.##}", 0, false, TooltipProperty.TooltipPropertyFlags.RundownResult));

            return new BasicTooltipViewModel(() => list);
        }

        protected virtual void ExecuteExecutiveAction()
        {
            DiplomacyCostCalculator.DetermineCostForDeclaringWar((Kingdom)Faction1, true).ApplyCost();
            DeclareWarAction.Apply(Faction1, Faction2);
        }

        protected void SendMessenger()
        {
            Events.Instance.OnMessengerSent(Faction2Leader.Hero);
            UpdateDiplomacyProperties();
        }

        protected void FormAlliance()
        {
            DeclareAllianceAction.Apply((Kingdom)Faction1, (Kingdom)Faction2, true);
            // FIXME: Why do we not UpdateDiplomacyProperties() here?
        }

        protected void ProposeNonAggressionPact()
        {
            FormNonAggressionPactAction.Apply((Kingdom)Faction1, (Kingdom)Faction2, true);
            UpdateDiplomacyProperties();
        }

        [DataSourceProperty]
        public bool IsAllianceAvailable
        {
            get => _isAllianceAvailable;
            set
            {
                if (value != _isAllianceAvailable)
                {
                    _isAllianceAvailable = value;
                    // FIXME: Was "isAlliance..." (lowercase 'i') -- prob a bug but need to verify this works
                    OnPropertyChanged(nameof(IsAllianceAvailable));
                }
            }
        }

        [DataSourceProperty]
        public int AllianceInfluenceCost
        {
            get => _allianceInfluenceCost;
            set
            {
                if (value != _allianceInfluenceCost)
                {
                    _allianceInfluenceCost = value;
                    OnPropertyChanged(nameof(AllianceInfluenceCost));
                }
            }
        }

        [DataSourceProperty]
        public int AllianceGoldCost
        {
            get => _allianceGoldCost;
            set
            {
                if (value != _allianceGoldCost)
                {
                    _allianceGoldCost = value;
                    // FIXME: This was incorrectly 'Influence' instead of 'Gold'. Nevertheless, check it.
                    OnPropertyChanged(nameof(AllianceGoldCost));
                }
            }
        }

        [DataSourceProperty]
        public bool IsNonAggressionPactAvailable
        {
            get => _isNonAggressionPactAvailable;
            set
            {
                if (value != _isNonAggressionPactAvailable)
                {
                    _isNonAggressionPactAvailable = value;
                    OnPropertyChanged(nameof(IsNonAggressionPactAvailable));
                }
            }
        }

        [DataSourceProperty]
        public int NonAggressionPactInfluenceCost
        {
            get => _nonAggressionPactInfluenceCost;
            set
            {
                if (value != _nonAggressionPactInfluenceCost)
                {
                    _nonAggressionPactInfluenceCost = value;
                    OnPropertyChanged(nameof(NonAggressionPactInfluenceCost));
                }
            }
        }

        [DataSourceProperty]
        public int NonAggressionPactGoldCost
        {
            get => _nonAggressionPactGoldCost;
            set
            {
                if (value != _nonAggressionPactGoldCost)
                {
                    _nonAggressionPactGoldCost = value;
                    OnPropertyChanged(nameof(NonAggressionPactGoldCost));
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
            get => _isMessengerAvailable;
            set
            {
                if (value != _isMessengerAvailable)
                {
                    _isMessengerAvailable = value;
                    OnPropertyChanged(nameof(IsMessengerAvailable));
                }
            }
        }

        [DataSourceProperty]
        public int InfluenceCost { get; protected set; }

        [DataSourceProperty]
        public bool IsOptionAvailable
        {
            get => _isOptionAvailable;
            set
            {
                if (value != _isOptionAvailable)
                {
                    _isOptionAvailable = value;
                    OnPropertyChanged(nameof(IsOptionAvailable));
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel? ActionHint
        {
            get => _actionHint;
            set
            {
                if (value != _actionHint)
                {
                    _actionHint = value;
                    OnPropertyChanged(nameof(ActionHint));
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel? AllianceHint
        {
            get => _allianceHint;
            set
            {
                if (value != _allianceHint)
                {
                    _allianceHint = value;
                    OnPropertyChanged(nameof(AllianceHint));
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel? NonAggressionPactHint
        {
            get => _nonAggressionPactHint;
            set
            {
                if (value != _nonAggressionPactHint)
                {
                    _nonAggressionPactHint = value;
                    OnPropertyChanged(nameof(NonAggressionPactHint));
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel? AllianceScoreHint
        {
            get => _allianceScoreHint;
            set
            {
                if (value != _allianceScoreHint)
                {
                    _allianceScoreHint = value;
                    OnPropertyChanged(nameof(AllianceScoreHint));
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel? NonAggressionPactScoreHint
        {
            get => _nonAggressionPactScoreHint;
            set
            {
                if (value != _nonAggressionPactScoreHint)
                {
                    _nonAggressionPactScoreHint = value;
                    OnPropertyChanged(nameof(NonAggressionPactScoreHint));
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
        public ImageIdentifierVM? Faction1Image { get; private set; }

        [DataSourceProperty]
        public ImageIdentifierVM? Faction2Image { get; private set; }

        [DataSourceProperty]
        public string NonAggressionPactHelpText { get; }

        [DataSourceProperty]
        public DiplomacyPropertiesVM? DiplomacyProperties { get; private set; }

        private bool _isOptionAvailable;
        private bool _isMessengerAvailable;
        private bool _isAllianceAvailable;
        private bool _isNonAggressionPactAvailable;
        private int _allianceInfluenceCost;
        private int _nonAggressionPactInfluenceCost;
        private HintViewModel? _allianceHint;
        private HintViewModel? _nonAggressionPactHint;
        private HintViewModel? _actionHint;
        private int _nonAggressionPactGoldCost;
        private BasicTooltipViewModel? _allianceScoreHint;
        private int _allianceGoldCost;
        private BasicTooltipViewModel? _nonAggressionPactScoreHint;
    }
}
